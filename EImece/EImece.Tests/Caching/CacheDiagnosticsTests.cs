using EImece.Tests.Infrastructure;
using EImece.Domain.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;

namespace EImece.Tests.Caching
{
    [TestClass]
    public class CacheDiagnosticsTests
    {
        private MemoryCacheProvider _cache;
        private string _prefix;

        [TestInitialize]
        public void Init()
        {
            CacheDiagnostics.Reset();
            _cache = new MemoryCacheProvider(TestNullLoggers.Create<MemoryCacheProvider>());
            _prefix = "setting:diag:" + Guid.NewGuid().ToString("N") + ":";
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cache.ClearByPrefix(_prefix);
            CacheDiagnostics.Reset();
        }

        [TestMethod]
        public void HitRatio_ZeroRequests_IsZero()
        {
            Assert.AreEqual(0d, CacheDiagnostics.HitRatioPercent);
            Assert.AreEqual(0, CacheDiagnostics.GetMetrics().TotalReads);
        }

        [TestMethod]
        public void HitRatio_IsHitsOverHitsPlusMisses()
        {
            CacheDiagnostics.RecordHit("Memory:a");
            CacheDiagnostics.RecordHit("Memory:a");
            CacheDiagnostics.RecordMiss("Memory:b");

            Assert.AreEqual(2, CacheDiagnostics.Hits);
            Assert.AreEqual(1, CacheDiagnostics.Misses);
            Assert.AreEqual(66.67d, CacheDiagnostics.HitRatioPercent);
        }

        [TestMethod]
        public void GetOrAdd_MissThenHit_IncrementsCountersAndTracksKey()
        {
            var key = _prefix + "general";
            var calls = 0;

            var first = _cache.GetOrAdd(key, () => { calls++; return "v1"; }, CachePolicy.Absolute(60));
            var second = _cache.GetOrAdd(key, () => { calls++; return "v2"; }, CachePolicy.Absolute(60));

            Assert.AreEqual("v1", first);
            Assert.AreEqual("v1", second);
            Assert.AreEqual(1, calls);
            Assert.AreEqual(1, CacheDiagnostics.Misses);
            Assert.AreEqual(1, CacheDiagnostics.Hits);
            Assert.AreEqual(1, CacheDiagnostics.Sets);
            Assert.AreEqual(2, CacheDiagnostics.GetMetrics().TotalReads);

            var entry = CacheDiagnostics.GetEntry(key);
            Assert.IsNotNull(entry);
            Assert.AreEqual(key, entry.Key);
            Assert.AreEqual("Settings", entry.Category);
            Assert.AreEqual(CacheDiagnostics.StatusActive, entry.Status);
            Assert.AreEqual(1, entry.HitCount);
            Assert.AreEqual(CacheDiagnostics.NotAvailable, entry.Size);
        }

        [TestMethod]
        public void Get_MissAndHit_IncrementSeparatelyFromSet()
        {
            var key = _prefix + "get";
            Assert.IsFalse(_cache.Get(key, out string _));
            Assert.AreEqual(1, CacheDiagnostics.Misses);

            _cache.Set(key, "secret-value-should-not-leak", CachePolicy.Absolute(60));
            Assert.AreEqual(1, CacheDiagnostics.Sets);

            Assert.IsTrue(_cache.Get(key, out string value));
            Assert.AreEqual("secret-value-should-not-leak", value);
            Assert.AreEqual(1, CacheDiagnostics.Hits);
        }

        [TestMethod]
        public void Clear_IncrementsRemovals_AndDropsKey()
        {
            var key = _prefix + "remove";
            _cache.Set(key, 9, CachePolicy.Absolute(60));
            _cache.Clear(key);

            Assert.IsTrue(CacheDiagnostics.Removals >= 1);
            Assert.IsNull(CacheDiagnostics.GetEntry(key));
            Assert.IsFalse(_cache.Get(key, out int _));
        }

        [TestMethod]
        public void Expiration_AbsolutePolicy_NextGetIsMiss()
        {
            var key = _prefix + "expire";
            _cache.Set(key, "warm", CachePolicy.Absolute(1));
            Thread.Sleep(1500);

            Assert.IsFalse(_cache.Get(key, out string _));
            Assert.IsTrue(CacheDiagnostics.Misses >= 1);
        }

        [TestMethod]
        public void RecordExpiration_IncrementsExpirationCounter()
        {
            CacheDiagnostics.RecordSet("Memory:" + _prefix + "x", typeof(string), CachePolicy.Absolute(5));
            CacheDiagnostics.RecordExpiration("Memory:" + _prefix + "x");
            Assert.AreEqual(1, CacheDiagnostics.Expirations);
            var entry = CacheDiagnostics.GetEntry(_prefix + "x");
            Assert.IsNotNull(entry);
            Assert.AreEqual(CacheDiagnostics.StatusExpired, entry.Status);
        }

        [TestMethod]
        public void QueryEntries_SearchCategoryStatusAndPagination()
        {
            _cache.Set(_prefix + "alpha", "a", CachePolicy.Absolute(60));
            _cache.Set(CacheKeys.ProductDetail(424242), "p", CachePolicy.Absolute(60));
            _cache.Set(CacheKeys.MenuTree(1), "m", CachePolicy.Absolute(60));

            var bySearch = CacheDiagnostics.QueryEntries(_prefix + "alpha", "all", "all", 1, 50);
            Assert.AreEqual(1, bySearch.TotalCount);
            Assert.AreEqual(_prefix + "alpha", bySearch.Entries[0].Key);

            var products = CacheDiagnostics.QueryEntries("", "Products", "all", 1, 50);
            Assert.IsTrue(products.Entries.Any(e => e.Key.Contains("product:detail")));

            var page1 = CacheDiagnostics.QueryEntries("", "all", "Active", 1, 2);
            Assert.IsTrue(page1.TotalCount >= 3);
            Assert.AreEqual(2, page1.Entries.Count);
            Assert.AreEqual(1, page1.Page);
            Assert.AreEqual(2, page1.PageSize);

            var page2 = CacheDiagnostics.QueryEntries("", "all", "Active", 2, 2);
            Assert.AreEqual(2, page2.Page);
            Assert.AreEqual(page2.TotalCount - 2, page2.Entries.Count);
        }

        [TestMethod]
        public void GetMatchingEntries_ReturnsAllKeysWithoutPageCap()
        {
            for (var i = 0; i < 5; i++)
            {
                _cache.Set(_prefix + "all" + i, i, CachePolicy.Absolute(60));
            }

            var paged = CacheDiagnostics.QueryEntries(_prefix + "all", "all", "all", 1, 2);
            var all = CacheDiagnostics.GetMatchingEntries(_prefix + "all", "all", "all");

            Assert.AreEqual(2, paged.Entries.Count);
            Assert.AreEqual(5, paged.TotalCount);
            Assert.AreEqual(5, all.Count);
        }

        [TestMethod]
        public void Snapshots_DoNotIncludeCachedValues()
        {
            var key = _prefix + "secret";
            _cache.Set(key, "super-secret-password", CachePolicy.Absolute(60));
            var snapshot = CacheDiagnostics.GetEntry(key);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(snapshot);

            Assert.IsFalse(json.IndexOf("super-secret-password", StringComparison.OrdinalIgnoreCase) >= 0);
            StringAssert.Contains(json, "\"Key\"");
        }

        [TestMethod]
        public void ToDisplayName_UsesReadableNameForSettingsKeys()
        {
            Assert.AreEqual("Settings", CacheDiagnostics.ToDisplayName("setting:all:lang1"));
            Assert.AreEqual("Website logo", CacheDiagnostics.ToDisplayName(CacheKeys.WebSiteLogoImage));
            Assert.AreEqual("Website logo", CacheDiagnostics.ToDisplayName(CacheKeys.WebSiteLogoImageLegacy));
            Assert.AreEqual("Product page", CacheDiagnostics.ToDisplayName(CacheKeys.ProductDetail(12)));
            Assert.AreEqual("Settings", CacheDiagnostics.ToDisplayName("Settings_12345"));
        }

        [TestMethod]
        public void QueryEntries_SearchMatchesDisplayName()
        {
            _cache.Set(CacheKeys.ProductDetail(424243), "p", CachePolicy.Absolute(60));
            var byName = CacheDiagnostics.QueryEntries("Product page", "all", "all", 1, 50);
            Assert.IsTrue(byName.Entries.Any(e => e.Key.Contains("product:detail")));
        }

        [TestMethod]
        public void GetOrAdd_RecordsCachedAndUncachedDurations()
        {
            var key = _prefix + "timed";
            _cache.GetOrAdd(key, () =>
            {
                Thread.Sleep(25);
                return "slow";
            }, CachePolicy.Absolute(60));
            _cache.GetOrAdd(key, () => "fast", CachePolicy.Absolute(60));

            var entry = CacheDiagnostics.GetEntry(key);
            Assert.IsNotNull(entry);
            Assert.IsTrue(entry.AvgUncachedMs.HasValue, "uncached factory time should be measured");
            Assert.IsTrue(entry.AvgCachedMs.HasValue, "cached lookup time should be measured");
            Assert.IsTrue(entry.AvgUncachedMs.Value > entry.AvgCachedMs.Value);
            Assert.IsTrue(entry.ImprovementPercent.HasValue);
            Assert.AreEqual(1, entry.Misses);
            Assert.AreEqual(1, entry.HitCount);

            var overview = CacheDiagnostics.BuildOverview();
            Assert.IsTrue(overview.Data.HasTiming);
            Assert.AreEqual(1, overview.Data.Hits);
            Assert.AreEqual(1, overview.Data.EstimatedDatabaseOperationsAvoided);
        }

        [TestMethod]
        public void OutputCache_HitsAndMisses_FeedPageOverviewWithoutInventingTimingWhenEmpty()
        {
            var empty = CacheDiagnostics.BuildOverview();
            Assert.IsFalse(empty.Page.HasTiming);
            Assert.AreEqual(0, empty.Page.Hits);

            CacheDiagnostics.RecordOutputMiss(TimeSpan.FromMilliseconds(180).Ticks);
            CacheDiagnostics.RecordOutputHit(TimeSpan.FromMilliseconds(12).Ticks);
            for (var i = 0; i < 6; i++)
            {
                CacheDiagnostics.RecordOutputHit(TimeSpan.FromMilliseconds(12).Ticks);
            }

            var overview = CacheDiagnostics.BuildOverview();
            Assert.AreEqual(7, overview.Page.Hits);
            Assert.AreEqual(1, overview.Page.Misses);
            Assert.IsTrue(overview.Page.HasTiming);
            Assert.IsTrue(overview.Page.AvgUncachedMs.HasValue);
            Assert.IsTrue(overview.Page.AvgCachedMs.HasValue);
            Assert.IsTrue(overview.Page.AvgUncachedMs.Value > overview.Page.AvgCachedMs.Value);
            Assert.AreEqual(CacheEffectivenessLevel.Effective, overview.Page.Effectiveness);
        }

        [TestMethod]
        public void BuildOverview_DoesNotHardcodePerformanceNumbers()
        {
            var overview = CacheDiagnostics.BuildOverview();
            Assert.AreEqual(0, overview.Combined.Hits);
            Assert.IsFalse(overview.Combined.HasTiming);
            Assert.IsNull(overview.Combined.SavedMs);
            Assert.IsNull(overview.Combined.ImprovementPercent);
        }
    }
}
