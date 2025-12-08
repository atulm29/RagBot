using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Models;

namespace RAGSERVERAPI.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UserInfo> RegisterAsync(RegisterRequest request);
    Task<bool> ValidateTokenAsync(string token);
    string GenerateJwtToken(User user, Tenant tenant, Role role);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IRoleRepository roleRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _roleRepository = roleRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive");
        }

        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId);
        var role = await _roleRepository.GetByIdAsync(user.RoleId);

        if (tenant == null || role == null)
        {
            throw new InvalidOperationException("User configuration is invalid");
        }

        await _userRepository.UpdateLastLoginAsync(user.Id);

        var token = GenerateJwtToken(user, tenant, role);

        return new LoginResponse(
            Token: token,
            RefreshToken: Guid.NewGuid().ToString(), // Implement proper refresh token logic
            User: new UserInfo(
                Id: user.Id,
                Email: user.Email,
                Username: user.Username,
                FirstName: user.FirstName,
                LastName: user.LastName,
                TenantId: tenant.Id,
                TenantName: tenant.Name,
                RoleId: role.Id,
                RoleName: role.Name
            )
        );
    }

    public async Task<UserInfo> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already registered");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
        var role = await _roleRepository.GetByIdAsync(request.RoleId);

        if (tenant == null || role == null)
        {
            throw new InvalidOperationException("Invalid tenant or role");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            RoleId = request.RoleId,
            Email = request.Email,
            Username = request.Username,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        return new UserInfo(
            Id: user.Id,
            Email: user.Email,
            Username: user.Username,
            FirstName: user.FirstName,
            LastName: user.LastName,
            TenantId: tenant.Id,
            TenantName: tenant.Name,
            RoleId: role.Id,
            RoleName: role.Name
        );
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured"));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateJwtToken(User user, Tenant tenant, Role role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtTokenConfig:secret"] ?? throw new InvalidOperationException("JWT secret not configured"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Username),
            new("TenantId", tenant.Id.ToString()),
            new("TenantCode", tenant.Code),
            new("RoleId", role.Id.ToString()),
            new("RoleCode", role.Code)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = _configuration["JwtTokenConfig:issuer"],
            Audience = _configuration["JwtTokenConfig:audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
