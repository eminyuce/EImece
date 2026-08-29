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
        public void GetConfigString_MissingKey_UsesDefaultAndLogsFallbackOnlyOnce()
        {
            AppConfig.ResetCacheForTests();
            const string key = "AppConfigTest_MissingKey_DoNotAddToWebConfig";

            var first = AppConfig.GetConfigString(key, "fallback-a");
            var second = AppConfig.GetConfigString(key, "fallback-b");
            var third = AppConfig.GetConfigString(key, "fallback-c");

            Assert.AreEqual("fallback-a", first);
            Assert.AreEqual("fallback-a", second);
            Assert.AreEqual("fallback-a", third);
            Assert.AreEqual(1, AppConfig.FallbackLogCount);
            Assert.AreEqual(1, AppConfig.CacheMissCount);
            Assert.AreEqual(2, AppConfig.CacheHitCount);
        }

        [TestMethod]
        public void GetConfigInt_MissingKey_UsesDefaultAndLogsFallbackOnlyOnce()
        {
            AppConfig.ResetCacheForTests();
            const string key = "AppConfigTest_MissingInt_DoNotAddToWebConfig";

            Assert.AreEqual(15, AppConfig.GetConfigInt(key, 15));
            Assert.AreEqual(15, AppConfig.GetConfigInt(key, 99));
            Assert.AreEqual(1, AppConfig.FallbackLogCount);
            Assert.AreEqual(1, AppConfig.CacheMissCount);
            Assert.AreEqual(1, AppConfig.CacheHitCount);
        }

        [TestMethod]
        public void GetConfigBool_MissingKey_UsesDefaultAndLogsFallbackOnlyOnce()
        {
            AppConfig.ResetCacheForTests();
            const string key = "AppConfigTest_MissingBool_DoNotAddToWebConfig";

            Assert.IsTrue(AppConfig.GetConfigBool(key, true));
            Assert.IsTrue(AppConfig.GetConfigBool(key, false));
            Assert.AreEqual(1, AppConfig.FallbackLogCount);
            Assert.AreEqual(1, AppConfig.CacheMissCount);
            Assert.AreEqual(1, AppConfig.CacheHitCount);
        }
    }
}
