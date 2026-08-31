using EImece.Tests.Infrastructure;
using EImece.Domain.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Caching;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ApplicationCacheClearerTests
    {
        [TestMethod]
        public void ClearMemoryCacheDefault_RemovesEntries()
        {
            var key = "test:appcache:" + Guid.NewGuid().ToString("N");
            MemoryCache.Default.Set(key, "value", DateTimeOffset.Now.AddMinutes(5));
            Assert.IsNotNull(MemoryCache.Default.Get(key));

            var removed = ApplicationCacheClearer.ClearMemoryCacheDefault();
            Assert.IsTrue(removed >= 1);
            Assert.IsNull(MemoryCache.Default.Get(key));
        }

        [TestMethod]
        public void LazyCacheProvider_ClearAll_ReturnsNonNegativeCount()
        {
            var cache = new LazyCacheProvider(TestNullLoggers.Create<LazyCacheProvider>());
            var key = "test:clearall:" + Guid.NewGuid().ToString("N");
            cache.Set(key, 1, CachePolicy.Absolute(60));

            var removed = cache.ClearAll();
            Assert.IsTrue(removed >= 1);
            Assert.IsFalse(cache.Get(key, out int _));
        }
    }
}
