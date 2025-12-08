using System.Data;
using Npgsql;

public class DataContext
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IConfiguration _configuration;

    public DataContext(NpgsqlDataSource dataSource, IConfiguration configuration)
    {
        _dataSource = dataSource;
        _configuration = configuration;
    }

    public NpgsqlConnection CreateConnection()
    {
        return _dataSource.CreateConnection();
    }
}

// public class DataContext
// {
//     private readonly IConfiguration _configuration;
//     private readonly string _connectionString;

//     public DataContext(IConfiguration configuration)
//     {
//         _configuration = configuration;
//         _connectionString = _configuration.GetConnectionString("DefaultConnection");
//     }

//     public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
// }
