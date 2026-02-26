using Npgsql;
using System.Data;

namespace CRM.Backend.Infra.Persistence;

public class DbConnectionFactory(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
