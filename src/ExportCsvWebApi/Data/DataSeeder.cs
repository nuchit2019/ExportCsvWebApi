using Dapper;
using System.Data;

namespace ExportCsvWebApi.Data
{
    public class DataSeeder
    {
        public static async Task SeedAsync(IDbConnection connection)
        {
            var createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Customers (
                Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(100) NOT NULL,
                Email NVARCHAR(100) NOT NULL,
                CreatedAt DATETIME NOT NULL
            );";

            var insertDataQuery = @"
            INSERT INTO Customers (Name, Email, CreatedAt)
            VALUES 
            ('John Doe', 'john@example.com', GETDATE()),
            ('Jane Smith', 'jane@example.com', GETDATE());";

            await connection.ExecuteAsync(createTableQuery);
            await connection.ExecuteAsync(insertDataQuery);
        }
    }
}
