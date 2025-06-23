using Dapper.Extensions;
using DapperExtensions.Attributes;
using DapperExtensions.Extensions;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DapperExtensions.Services
{
    public static class BulkUploadTempTableService
    {
        internal static async Task InsertDynamicTempTable<TIn>(SqlConnection connection, IEnumerable<TIn> entities, int batchSize = 1000, int numberOfRetries = 5,
            SqlTransaction transaction = null, int? commandTimeout = null) where TIn : class
        {
            var tempTableName = GetTempTableName<TIn>();
            var atts = GetColumnsAndTypes<TIn>();
            var attsString = string.Join(", ", atts.Select(kvp => $"[{kvp.Key}] {kvp.Value}"));
            var sql = $"CREATE TABLE {tempTableName} ({attsString})";
            await connection.ExecuteWithRetryAsync(sql);

            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                using (var reader = ObjectReader.Create(entities)) // this was list {Item = entity}
                {
                    int i = 0;
                    foreach (var kvp in atts)
                    {
                        bulkCopy.ColumnMappings.Add(kvp.Key, i);
                        i++;
                    }

                    bulkCopy.BatchSize = batchSize;
                    bulkCopy.DestinationTableName = tempTableName;
                    await bulkCopy.WriteToServerAsync(reader);
                    await connection.ExecuteWithRetryAsync(sql, null, transaction, commandTimeout: commandTimeout)
                        .WithRetry(numberOfRetries);
                }
            }
        }

        private static string GetTempTableName<TIn>() where TIn : class
        {
            var prop = typeof(TIn).GetCustomAttribute<TempTableName>();

            string tempTableName;
            var isOverride = false;

            if (prop != null)
            {
                isOverride = true;
                tempTableName = prop.NameOverride;
            }
            else
            {
                tempTableName = typeof(TIn).Name;
            }

            if (tempTableName.ContainsSQLInjectionKeywords())
            {
                var overrideString = isOverride ? " override" : string.Empty;
                throw new ArgumentException($"Attempted to inject SQL into temp table via class name{overrideString}: {tempTableName}", nameof(tempTableName));
            }

            return $"#{tempTableName}";
        }

        private static Dictionary<string, string> GetColumnsAndTypes<TIn>() where TIn : class
        {
            var res = new Dictionary<string, string>();
            var props = typeof(TIn).GetProperties();

            foreach (var prop in props)
            {
                var col = prop.GetCustomAttribute<TempTable>();

                if (col != null)
                {
                    var isOverride = false;
                    var tempTableColumnName = string.Empty;

                    if (!string.IsNullOrWhiteSpace(col.NameOverride))
                    {
                        isOverride = true;
                        tempTableColumnName = col.NameOverride;
                    }
                    else
                    {
                        tempTableColumnName = prop.Name;
                    }

                    if (tempTableColumnName.ContainsSQLInjectionKeywords())
                    {
                        var overrideString = isOverride ? " override" : string.Empty;
                        var msg = $"Attempted to inject SQL into temp table query via attribute name{overrideString}: {tempTableColumnName}";
                        throw new ArgumentException(msg, nameof(prop.Name));
                    }

                    res.Add(prop.Name, col.GetImplementation());
                }
            }

            if (res.IsNullOrEmpty())
            {
                throw new ArgumentException($"Class {typeof(TIn).Name} did not contain any temp table attributes.", typeof(TIn).Name);
            }

            return res;
        }
    }
}