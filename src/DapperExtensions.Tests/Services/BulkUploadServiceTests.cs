using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Collections.Generic;
using System;

namespace DapperExtensions.Tests.Services
{
    internal class BulkUploadServiceTests
    {
        private readonly SqliteConnection _connection;

        public BulkUploadServiceTests()
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
        public async Task CanUploadData()
        {
            await _connection.ExecuteAsync(@"
                CREATE TABLE DapperTableTest (
                    Id INT IDENTITY,
                    IsVarchar VARCHAR(50) NOT NULL,
                    IsBit BIT NOT NULL,
                    IsNullableBit BIT NULL,
                    IsDecimal DECIMAL(5, 2) NOT NULL,
                    IsNullableDecimal DECIMAL(7, 3) NULL,
                    IsTinyInt TINYINT NOT NULL,
                    IsSmallInt SMALLINT NOT NULL,
                    IsUShort SMALLINT NOT NULL,
                    IsInt INT NOT NULL,
                    IsUInt
                    IsNullableInt INT NULL

                );
            ");


        }

        private class DapperTableTest
        {
            /// <summary>
            /// Mapped to identity in sql db, 
            /// </summary>
            public int Id { get; internal set; }
            public string IsVarchar { get; set; }
            public bool IsBit { get; set; }
            public bool? IsNullableBit { get; set; }
            public decimal IsDecimal { get; set; }
            public decimal? IsNullableDecimal { get; set; }
            public sbyte IsSByte { get; set; }
            public byte IsByte { get; set; }
            public short IsInt16 { get; set; }
            public ushort IsUInt16 { get; set; }
            public int IsInt { get; set; }
            public uint IsUInt { get; set; }
            public long IsInt64 { get; set; }
            public ulong IsUInt64 { get; set; }
        }
    }
}