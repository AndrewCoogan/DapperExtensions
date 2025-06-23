using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DapperExtensions.Tests
{
    internal sealed class SqlExceptionHelper
    {
        internal static SqlException Generate(int errorNumber)
        {
            var ex = (SqlException)RuntimeHelpers.GetUninitializedObject(typeof(SqlException));
            var errors = GenerateSqlErrorCollection(errorNumber);
            SetPrivateFieldValue(ex, "_errors", errors);
            return ex;
        }

        internal static SqlErrorCollection GenerateSqlErrorCollection(int errorNumber)
        {
            var t = typeof(SqlErrorCollection);
            var col = (SqlErrorCollection)RuntimeHelpers.GetUninitializedObject(t);
            SetPrivateFieldValue(col, "_errors", new List<object>());
            var sqlError = GenerateSqlError(errorNumber);
            var method = t.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(col, new object[] { sqlError });
                return col;
            }
            throw new Exception("Could not get method 'Add'");
        }

        private static SqlError GenerateSqlError(int errorNumber)
        {
            var sqlError = (SqlError)RuntimeHelpers.GetUninitializedObject(typeof(SqlError));
            SetPrivateFieldValue(sqlError, "_number", errorNumber);
            SetPrivateFieldValue(sqlError, "_message", string.Empty);
            SetPrivateFieldValue(sqlError, "_procedure", string.Empty);
            SetPrivateFieldValue(sqlError, "_server", string.Empty);
            SetPrivateFieldValue(sqlError, "_source", string.Empty);
            return sqlError;
        }

        private static void SetPrivateFieldValue(object obj, string field, object val)
        {
            var member = obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            if (member != null)
            {
                member.SetValue(obj, val);
            }
            else
            {
                throw new Exception($"Could not set private field {field} on object: {obj.ToString()}");
            }
        }
    }
}