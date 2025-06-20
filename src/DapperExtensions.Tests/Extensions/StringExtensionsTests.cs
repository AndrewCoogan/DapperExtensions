using DapperExtensions.Extensions;
using NUnit.Framework;

namespace DapperExtensions.Tests.Extensions
{
    internal class StringExtensionsTests
    {
        private string? sqlInjectDrop;
        private string? sqlTempTableCreationFromVarchar;
        private string? sqlTempTableCreationFromInt;

        [SetUp]
        public void Setup()
        {
            sqlInjectDrop = "--DROP table";
            sqlTempTableCreationFromVarchar = "varchar(255)";
            sqlTempTableCreationFromInt = "int";
        }

        [Test]
        public void CanDetectSQLInjectionForTempTable()
        {
            Assert.That(sqlInjectDrop.ContainsSQLInjectionKeywords(), Is.True);
            Assert.That(sqlTempTableCreationFromInt.ContainsSQLInjectionKeywords(), Is.False);
            Assert.That(sqlTempTableCreationFromVarchar.ContainsSQLInjectionKeywords(), Is.False);
        }
    }
}