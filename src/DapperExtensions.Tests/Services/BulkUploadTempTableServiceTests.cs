using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Collections.Generic;

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
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Dispose();
        }

        [Test]
        public async Task CanGetData()
        {
            await _connection.ExecuteAsync(@"
                CREATE TABLE DapperTableTest (
                    Id INT IDENTITY,
                    IsVarchar VARCHAR(50) NOT NULL,
                    IsBit BIT NOT NULL,
                    IsNullableBit BIT NULL,
                    IsDecimal DECIMAL NOT NULL,


                );
            ");

            var sql = "SELECT * FROM Departments";
            var res = await _connection.QueryAsync<DapperTableTest>(sql);
            Assert.That(res, Is.Not.Null);
            Assert.That(res, Has.Exactly(3).Items);
            Assert.That(res, Is.InstanceOf<IEnumerable<DapperTableTest>>());
        }

        private class DapperTableTest
        {
            public int Id { get; internal set; }
            public string IsVarchar { get; set; }
            public bool IsBit { get; set; }
            public bool? IsNullableBit { get; set; }
            public decimal IsDecimal { get; set; }

        }
    }
}