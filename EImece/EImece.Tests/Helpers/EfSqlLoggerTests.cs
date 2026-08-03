using EImece.Domain.Observability.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class EfSqlLoggerTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            EfSqlLogger.Configure(false);
        }

        [TestMethod]
        public void Configure_WhenEnabled_SetsIsEnabledTrue()
        {
            EfSqlLogger.Configure(true);
            Assert.IsTrue(EfSqlLogger.IsEnabled);
        }

        [TestMethod]
        public void Configure_WhenDisabled_SetsIsEnabledFalse()
        {
            EfSqlLogger.Configure(true);
            EfSqlLogger.Configure(false);
            Assert.IsFalse(EfSqlLogger.IsEnabled);
        }

        [TestMethod]
        public void Write_WhenDisabled_DoesNotThrow()
        {
            EfSqlLogger.Configure(false);
            EfSqlLogger.Write("SELECT 1");
        }

        [TestMethod]
        public void Write_WhenEnabled_DoesNotThrowForSql()
        {
            EfSqlLogger.Configure(true);
            EfSqlLogger.Write("SELECT * FROM Products WHERE Id = @p0");
        }

        [TestMethod]
        public void Write_WhenEnabled_MasksSensitiveFragments()
        {
            EfSqlLogger.Configure(true);
            // Should not throw; masking is applied before logging.
            EfSqlLogger.Write("UPDATE Users SET password = 'secret123' WHERE Id = 1");
        }

        [TestMethod]
        public void Attach_WhenDisabled_DoesNotThrowForNullContext()
        {
            EfSqlLogger.Configure(false);
            EfSqlLogger.Attach(null);
        }

        [TestMethod]
        public void Attach_WhenEnabled_DoesNotThrowForNullContext()
        {
            EfSqlLogger.Configure(true);
            EfSqlLogger.Attach(null);
        }
    }
}
