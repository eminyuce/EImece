using EImece.Domain.Observability.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class SensitiveDataMaskerTests
    {
        [TestMethod]
        public void Mask_ReplacesPasswordAssignments()
        {
            var masked = SensitiveDataMasker.Mask("password=super-secret token=abc123");
            Assert.IsFalse(masked.Contains("super-secret"), "password value should be masked: " + masked);
            Assert.IsFalse(masked.Contains("abc123"), "token value should be masked: " + masked);
            StringAssert.Contains(masked, "password=***");
            StringAssert.Contains(masked, "token=***");
        }

        [TestMethod]
        public void Mask_ReplacesBearerTokens()
        {
            var masked = SensitiveDataMasker.Mask("Authorization: Bearer eyJhbGciOiJIUzI1NiJ9.abc");
            Assert.IsFalse(masked.Contains("eyJhbGciOiJIUzI1NiJ9"), "jwt should be masked: " + masked);
            Assert.IsTrue(
                masked.IndexOf("Bearer ***", System.StringComparison.OrdinalIgnoreCase) >= 0
                || masked.IndexOf("authorization=***", System.StringComparison.OrdinalIgnoreCase) >= 0,
                "authorization/bearer should be masked: " + masked);
        }

        [TestMethod]
        public void Mask_ReplacesCardLikeNumbers()
        {
            var masked = SensitiveDataMasker.Mask("card 4111111111111111");
            Assert.IsFalse(masked.Contains("4111111111111111"));
            Assert.IsTrue(masked.Contains("****-****-****-****"));
        }

        [TestMethod]
        public void Mask_ReplacesConnectionStringSecrets()
        {
            var masked = SensitiveDataMasker.Mask("Server=.;Database=EImece;Password=hunter2;User ID=sa");
            Assert.IsFalse(masked.Contains("hunter2"));
            Assert.IsTrue(masked.Contains("Password=***"));
        }

        [TestMethod]
        public void Mask_NullOrEmpty_ReturnsSame()
        {
            Assert.IsNull(SensitiveDataMasker.Mask(null));
            Assert.AreEqual(string.Empty, SensitiveDataMasker.Mask(string.Empty));
        }
    }
}
