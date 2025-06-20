using DapperExtensions.Helpers;
using NUnit.Framework;

namespace DapperExtensions.Tests.Extensions
{
    internal class QueryHelperTests
    {
        [Test]
        public void CanGetAllQueries()
        {
            Assert.That(QueryHelper.TableSchema.Select, Is.Not.Null);
            Assert.That(QueryHelper.TempTableSchema.Select, Is.Not.Null);
        }
    }
}