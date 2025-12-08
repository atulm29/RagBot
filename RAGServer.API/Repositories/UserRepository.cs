using System.Data;
using Dapper;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Services;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid userId);
    Task<User> CreateAsync(User user);
    Task UpdateLastLoginAsync(Guid userId);
}

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;

    public UserRepository(DataContext dataContext)
    {
        _context = dataContext;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM users WHERE email = @Email";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM users WHERE id = @UserId";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    public async Task<User> CreateAsync(User user)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO users (id, tenantid, roleid, email, username, passwordhash, firstname, lastname, isactive, createdat, updatedat)
            VALUES (@Id, @TenantId, @RoleId, @Email, @Username, @PasswordHash, @FirstName, @LastName, @IsActive, @CreatedAt, @UpdatedAt)
            RETURNING *";

        return await connection.QuerySingleAsync<User>(sql, user);
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        using var connection = _context.CreateConnection();
        var sql = "UPDATE users SET lastlogin = @LastLogin WHERE id = @UserId";
        await connection.ExecuteAsync(sql, new { UserId = userId, LastLogin = DateTime.UtcNow });
    }
}
