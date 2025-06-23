using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Data.Sqlite;
using Dapper;

namespace DapperExtensions.Tests.Services
{
    internal class BulkUploadTempTableServiceTests
    {
        private readonly SqliteConnection _connection;

        public BulkUploadTempTableServiceTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
        }

        [SetUp]
        public async Task SetUp()
        {
            await _connection.OpenAsync();
            await SetUpSqlData();
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Dispose();
        }

        private async Task SetUpSqlData()
        {
            await _connection.ExecuteAsync(@"
                CREATE TABLE Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT,
                    Email TEXT,
                    DepartmentId INTEGER
                );

                CREATE TABLE Departments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT
                );

                -- Seed data
                INSERT INTO Departments (Name) VALUES ('IT'), ('HR'), ('Sales');
                INSERT INTO Users (Name, Email, DepartmentId) VALUES 
                    ('John Doe', 'john@example.com', 1),
                    ('Jane Smith', 'jane@example.com', 2),
                    ('Bob Johnson', 'bob@example.com', 1),
                    ('Alice Brown', 'alice@example.com', 3);
            ");
        }

        [Test]
        public async Task CanGetData()
        {
            var sql = "SELECT * FROM Departments";
            var res = await _connection.QueryAsync<TestClass>(sql);
            Assert.That(res, Is.Not.Null);
        }

        private class TestClass
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }
    }
}