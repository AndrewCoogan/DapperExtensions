using Dapper.Extensions;
using DapperExtensions.Extensions;
using DapperExtensions.Helpers;
using DapperExtensions.Models;
using FastMember;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DapperExtensions.Services
{
    public static class BulkUploadTempTableService
    {
        internal static async Task InsertDynamicTempTable<TIn>(SqlConnection connection, IEnumerable<TIn> entities, int batchSize = 1000, int numberOfRetries = 5,
            SqlTransaction transaction = null, int? commandTimeout = null) where TIn : new()
        {
            // use reflection to loop over the items, and use that values that have headers
            var propNames = typeof(TIn).GetProperties().Select(x => x.Name);

            var tmp = "#Temp";
            await connection.ExecuteWithRetryAsync($"CREATE TABLE {tmp}");

            // format TIn to be an object of just tagged entities?

            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, externalTransaction: transaction))
            {
                using (var reader = ObjectReader.Create(entities)) // this was list {Item = entity}
                {
                    int i = 0;
                    foreach (var prop in propNames)
                    {
                        bulkCopy.ColumnMappings.Add(prop, i);
                        i++;
                    }

                    var sql = "";
                    bulkCopy.BatchSize = batchSize;
                    bulkCopy.DestinationTableName = tmp;
                    await bulkCopy.WriteToServerAsync(reader);
                    await connection.ExecuteWithRetryAsync(sql, null, transaction, commandTimeout: commandTimeout)
                        .WithRetry(numberOfRetries);
                }
            }
        }
    }
}