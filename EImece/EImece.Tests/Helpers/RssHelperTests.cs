using EImece.Domain.Caching;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.RazorCustomRssTemplate;
using EImece.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class RssHelperTests
    {
        private MemoryCacheProvider _cache;
        private string _synKey;

        [TestInitialize]
        public void Init()
        {
            CacheDiagnostics.Reset();
            _cache = new MemoryCacheProvider(TestNullLoggers.Create<MemoryCacheProvider>());
            _synKey = "syn-" + Guid.NewGuid().ToString("N");
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cache.ClearByPrefix(CacheKeys.RssPrefix);
            CacheDiagnostics.Reset();
        }

        [TestMethod]
        public void SetAndGetRssInEmail_UsesProvider_NotASecondCacheHost()
        {
            RssHelper.SetRssInEmail(_synKey, new RssInEmail { rssUrl = "https://example.test/feed", isSubjectSource = true }, _cache);

            var list = RssHelper.GetListRssInEmail(_synKey, _cache);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("https://example.test/feed", list[0].rssUrl);

            Assert.IsTrue(_cache.Get(CacheKeys.RssEmail(_synKey), out System.Collections.Generic.List<RssInEmail> stored));
            Assert.AreEqual(1, stored.Count);
        }

        [TestMethod]
        public void GetListRssInEmail_WithoutProvider_ReturnsEmpty()
        {
            var list = RssHelper.GetListRssInEmail(_synKey, null);
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void ContentInvalidation_ClearsRssKeys()
        {
            RssHelper.SetRssInEmail(_synKey, new RssInEmail { rssUrl = "u" }, _cache);
            Assert.IsTrue(_cache.Get(CacheKeys.RssEmail(_synKey), out System.Collections.Generic.List<RssInEmail> _));

            var removed = _cache.ClearByPrefix(CacheKeys.RssPrefix);
            Assert.IsTrue(removed >= 1);
            Assert.IsFalse(_cache.Get(CacheKeys.RssEmail(_synKey), out System.Collections.Generic.List<RssInEmail> _));
        }

        [TestMethod]
        public void ResolveCategory_RssArea_IsRss()
        {
            Assert.AreEqual("Rss", CacheDiagnostics.ResolveCategory(CacheKeys.RssEmail("x")));
        }
    }
}
