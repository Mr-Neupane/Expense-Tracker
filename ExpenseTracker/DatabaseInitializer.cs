using Npgsql;

namespace ExpenseTracker;

public static class DatabaseInitializer
{
    public static void EnsureDatabaseAndMigrations(IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(baseConnectionString))
            throw new InvalidOperationException("Database name not configured.");

        var csb = new NpgsqlConnectionStringBuilder(baseConnectionString);
        var databaseName = csb.Database;

        csb.Database = "postgres";
        var adminConnectionString = csb.ConnectionString;
        using (var connection = new NpgsqlConnection(adminConnectionString))
        {
            connection.Open();

            using var cmd = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = @dbname",
                connection);

            cmd.Parameters.AddWithValue("dbname", databaseName);

            var exists = cmd.ExecuteScalar();

            if (exists == null)
            {
                using var createCmd = new NpgsqlCommand(
                    $"CREATE DATABASE \"{databaseName}\"",
                    connection);

                createCmd.ExecuteNonQuery();
            }
        }
    }
}