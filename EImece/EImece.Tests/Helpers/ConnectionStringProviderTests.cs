using System;
using System.Configuration;
using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Assert.AreEqual("EIMECE_DB_CONNECTION_STRING", ConnectionStringProvider.EnvironmentVariableName);
            Assert.AreEqual("EIMECE_DB_CONNECTION_STRING", Domain.Constants.DbConnectionEnvironmentVariable);
        }
    }
}
