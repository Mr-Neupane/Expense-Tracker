using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker
{
    public static class DapperConnectionProvider
    {
        private static string _connectionString;

        // Call this once during startup to initialize the connection string
        public static void Initialize(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public static IDbConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("DapperConnectionProvider is not initialized. Call Initialize() first.");

            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}