using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.IO;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ConnectionStringProviderTests
    {
        [TestMethod]
        public void Validate_RejectsPlaceholderServer()
        {
            try
            {
                ConnectionStringProvider.Validate(
                    "Data Source=YOUR_SERVER;Initial Catalog=EImece;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;",
                    "EImeceDbConnection");
                Assert.Fail("Expected ConfigurationErrorsException for placeholder connection string.");
            }
            catch (ConfigurationErrorsException ex)
            {
                StringAssert.Contains(ex.Message, "YOUR_SERVER");
            }
        }

        [TestMethod]
        public void Validate_RejectsEmpty()
        {
            try
            {
                ConnectionStringProvider.Validate("   ", "EImeceDbConnection");
                Assert.Fail("Expected ConfigurationErrorsException for empty connection string.");
            }
            catch (ConfigurationErrorsException)
            {
                // expected
            }
        }

        [TestMethod]
        public void Validate_AcceptsConcreteIntegratedSecurityString()
        {
            var result = ConnectionStringProvider.Validate(
                "Data Source=localhost;Initial Catalog=EImece;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;",
                "EImeceDbConnection");

            Assert.IsTrue(result.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [TestMethod]
        public void EnvironmentVariableName_IsStable()
        {
            // Intentional drift guard: both sides are compile-time constants today,
            // but this test fails if either literal ever changes independently.
#pragma warning disable MSTEST0032 // Assertion condition is known to be always true
            Assert.AreEqual("EIMECE_DB_CONNECTION_STRING", ConnectionStringProvider.EnvironmentVariableName);
            Assert.AreEqual("EIMECE_DB_CONNECTION_STRING", Domain.Constants.DbConnectionEnvironmentVariable);
#pragma warning restore MSTEST0032 // Assertion condition is known to be always true
        }

        [TestMethod]
        public void TryReadNamedConnectionStringFromFile_ReadsConfigSourceStyleFile()
        {
            var path = Path.Combine(Path.GetTempPath(), "EImece-cs-" + Guid.NewGuid().ToString("N") + ".config");
            File.WriteAllText(path,
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<connectionStrings>" +
                "  <add name=\"EImeceDbConnection\" connectionString=\"Data Source=parent-sql;Initial Catalog=EImece;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;\" providerName=\"System.Data.SqlClient\" />" +
                "</connectionStrings>");
            try
            {
                var value = ConnectionStringProvider.TryReadNamedConnectionStringFromFile(path, "EImeceDbConnection");
                StringAssert.Contains(value, "parent-sql");
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void TryReadNamedConnectionStringFromFile_ReturnsNullWhenMissing()
        {
            var value = ConnectionStringProvider.TryReadNamedConnectionStringFromFile(
                Path.Combine(Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid().ToString("N") + ".config"),
                "EImeceDbConnection");
            Assert.IsNull(value);
        }
    }
}
