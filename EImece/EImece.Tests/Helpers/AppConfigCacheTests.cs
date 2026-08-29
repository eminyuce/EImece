using EImece.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class AppConfigCacheTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            AppConfig.ResetCacheForTests();
        }

        [TestMethod]
        public void GetConfigString_MissingKey_UsesFirstDefaultOnLaterReads()
        {
            AppConfig.ResetCacheForTests();
            const string key = "AppConfigTest_MissingKey_DoNotAddToWebConfig";

            var first = AppConfig.GetConfigString(key, "fallback-a");
            var second = AppConfig.GetConfigString(key, "fallback-b");
            var third = AppConfig.GetConfigString(key, "fallback-c");

            Assert.AreEqual("fallback-a", first);
            Assert.AreEqual("fallback-a", second);
            Assert.AreEqual("fallback-a", third);
        }

        [TestMethod]
        public void GetConfigInt_MissingKey_UsesFirstDefaultOnLaterReads()
        {
            AppConfig.ResetCacheForTests();
            const string key = "AppConfigTest_MissingInt_DoNotAddToWebConfig";

            Assert.AreEqual(15, AppConfig.GetConfigInt(key, 15));
            Assert.AreEqual(15, AppConfig.GetConfigInt(key, 99));
        }

        [TestMethod]
        public void GetConfigBool_MissingKey_UsesFirstDefaultOnLaterReads()
        {
            AppConfig.ResetCacheForTests();
            const string key = "AppConfigTest_MissingBool_DoNotAddToWebConfig";

            Assert.IsTrue(AppConfig.GetConfigBool(key, true));
            Assert.IsTrue(AppConfig.GetConfigBool(key, false));
        }
    }
}
