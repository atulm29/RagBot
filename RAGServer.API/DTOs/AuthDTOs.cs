
namespace RAGSERVERAPI.DTOs;


public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    string RefreshToken,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Email,
    string Username,
    string? FirstName,
    string? LastName,
    Guid TenantId,
    string TenantName,
    Guid RoleId,
    string RoleName
);

public record RegisterRequest(
    string Email,
    string Username,
    string Password,
    string FirstName,
    string LastName,
    Guid TenantId,
    Guid RoleId
);
