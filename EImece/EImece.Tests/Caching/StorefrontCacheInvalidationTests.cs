using EImece.Domain.Caching;
using EImece.Domain.Models.Enums;
using EImece.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Caching
{
    /// <summary>
    /// Verifies the storefront cache-key contract: every key the services write falls under
    /// the hierarchical family prefix that the matching invalidation routine clears
    /// (InvalidateProductListCaches, InvalidateTagCaches, InvalidateMenuCaches,
    /// InvalidateCategoryCaches, InvalidateBrandCaches). Guards against the regression class
    /// where ad-hoc keys (TypeFullName-based, MenuNavTree-*, ...) escaped ClearByPrefix and
    /// stayed stale until TTL expiry.
    /// </summary>
    [TestClass]
    public class StorefrontCacheInvalidationTests
    {
        private MemoryCacheProvider _cache;
        private Random _rnd;

        // Prefix sets exactly as used by the service invalidators.
        private static readonly string[] ProductInvalidationPrefixes =
        {
            CacheKeys.ProductListPrefix,     // product:list:
            CacheKeys.ProductSearchPrefix,   // product:search:
            CacheKeys.ProductDetailPrefix,   // product:detail:
            CacheKeys.ProductRelatedPrefix,  // product:related:
            CacheKeys.CategoryPrefix         // category:
        };

        private static readonly string[] TagInvalidationPrefixes =
        {
            CacheKeys.TagPrefix,             // tag:
            CacheKeys.ProductListPrefix,
            CacheKeys.StoryPrefix
        };

        private static readonly string[] MenuInvalidationPrefixes = { CacheKeys.MenuPrefix };
        private static readonly string[] BrandInvalidationPrefixes = { CacheKeys.BrandPrefix, CacheKeys.ProductListPrefix };

        /// <summary>Mirrors BaseService.AsyncCacheKeySuffix (protected) — async twins use separate keys.</summary>
        private const string AsyncSuffix = "-async";

        [TestInitialize]
        public void Init()
        {
            _cache = new MemoryCacheProvider(TestNullLoggers.Create<MemoryCacheProvider>());
            _rnd = new Random(unchecked((int)DateTime.UtcNow.Ticks));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cache.ClearByPrefix(CacheKeys.ProductArea + ":");
            _cache.ClearByPrefix(CacheKeys.CategoryArea + ":");
            _cache.ClearByPrefix(CacheKeys.TagArea + ":");
            _cache.ClearByPrefix(CacheKeys.MenuArea + ":");
            _cache.ClearByPrefix(CacheKeys.BrandArea + ":");
            _cache.ClearByPrefix("activelist:");
        }

        #region Key contract

        [TestMethod]
        public void ProductDetailKeys_AreUnderDetailFamily()
        {
            int id = _rnd.Next(100000, int.MaxValue - 1);
            StringAssert.StartsWith(CacheKeys.ProductDetail(id), CacheKeys.ProductDetailPrefix);
            StringAssert.StartsWith(CacheKeys.ProductDetailAsync(id), CacheKeys.ProductDetailPrefix);
        }

        [TestMethod]
        public void CategoryCanonicalKeys_AreUnderCategoryFamily()
        {
            int lang = _rnd.Next(1, 100);
            StringAssert.StartsWith(CacheKeys.CategoryMainPage(lang), CacheKeys.CategoryPrefix);
            StringAssert.StartsWith(CacheKeys.CategoryNavigationTree(lang), CacheKeys.CategoryPrefix);
            StringAssert.StartsWith(CacheKeys.CategoryMainPageAsync(lang), CacheKeys.CategoryPrefix);

            // Keys written by ProductCategoryService after the canonical migration.
            StringAssert.StartsWith(CacheKeys.CategoryPrefix + "admintree:" + true + ":lang" + lang, CacheKeys.CategoryPrefix);
            StringAssert.StartsWith(CacheKeys.CategoryPrefix + "navtree:lang" + lang, CacheKeys.CategoryPrefix);
            StringAssert.StartsWith(CacheKeys.CategoryPrefix + "mainpageentities:lang" + lang, CacheKeys.CategoryPrefix);
            StringAssert.StartsWith(CacheKeys.CategoryPrefix + "children:id42", CacheKeys.CategoryPrefix);
        }

        [TestMethod]
        public void MenuAndTagAndBrandCanonicalKeys_AreUnderTheirFamilies()
        {
            int lang = _rnd.Next(1, 100);
            Assert.IsTrue((CacheKeys.MenuPrefix + "navtree:lang" + lang).StartsWith(CacheKeys.MenuPrefix, StringComparison.Ordinal));
            Assert.IsTrue((CacheKeys.MenuPrefix + "admintree:True:lang" + lang).StartsWith(CacheKeys.MenuPrefix, StringComparison.Ordinal));

            Assert.IsTrue((CacheKeys.TagPrefix + "admintags:lang" + lang).StartsWith(CacheKeys.TagPrefix, StringComparison.Ordinal));
            Assert.IsTrue((CacheKeys.TagPrefix + "storycounts:lang" + lang + ":min1").StartsWith(CacheKeys.TagPrefix, StringComparison.Ordinal));
            Assert.IsTrue((CacheKeys.TagPrefix + "entitycounts:lang" + lang + ":min1").StartsWith(CacheKeys.TagPrefix, StringComparison.Ordinal));
            Assert.IsTrue((CacheKeys.TagPrefix + "storefront:lang" + lang).StartsWith(CacheKeys.TagPrefix, StringComparison.Ordinal));

            int catId = _rnd.Next(1, 10000);
            Assert.IsTrue((CacheKeys.BrandPrefix + "list:cat" + catId + ":lang" + lang).StartsWith(CacheKeys.BrandPrefix, StringComparison.Ordinal));
        }

        [TestMethod]
        public void SearchKey_NormalizesTerm_SoEquivalentQueriesShareEntry()
        {
            var a = CacheKeys.ProductSearch("  Test ÜRÜN! ", 1, 10, 1, SortingType.Default);
            var b = CacheKeys.ProductSearch("test ürün_", 1, 10, 1, SortingType.Default);
            Assert.AreEqual(a, b);
        }

        #endregion

        #region Family eviction (service invalidator semantics)

        [TestMethod]
        public void ProductListInvalidation_DropsListSearchDetailRelatedAndCategoryEntries()
        {
            int productId = _rnd.Next(100000, int.MaxValue - 1);
            int catId = _rnd.Next(100000, int.MaxValue - 1);
            int lang = _rnd.Next(1, 100);

            Seed(CacheKeys.MainPageProducts(1, lang), "v");
            Seed(CacheKeys.ActiveProducts(lang), "v");
            Seed(CacheKeys.ActiveProductsAsync(lang), "v");
            Seed(CacheKeys.ProductSearch("phone", 1, 10, lang, SortingType.Newest), "v");
            Seed(CacheKeys.ProductDetail(productId), "detail-sync");
            Seed(CacheKeys.ProductDetailAsync(productId), "detail-async");

            Evict(ProductInvalidationPrefixes);

            Assert.IsFalse(_cache.Get(CacheKeys.MainPageProducts(1, lang), out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.ActiveProducts(lang), out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.ActiveProductsAsync(lang), out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.ProductSearch("phone", 1, 10, lang, SortingType.Newest), out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.ProductDetail(productId), out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.ProductDetailAsync(productId), out string _));
        }

        [TestMethod]
        public void TagInvalidation_DropsTagAdminAndCountLists_ButNotSettings()
        {
            int lang = _rnd.Next(1, 100);
            string settingsKey = "setting:keyvalues:lang" + lang;

            Seed(CacheKeys.TagPrefix + "admintags:lang" + lang, "v");
            Seed(CacheKeys.TagPrefix + "storycounts:lang" + lang + ":min1", "v");
            Seed(CacheKeys.TagPrefix + "entity_counts:lang" + lang + ":min1", "v");
            Seed(CacheKeys.TagPrefix + "storefront:lang" + lang, "v");
            Seed(settingsKey, "keep-me");

            Evict(TagInvalidationPrefixes);

            Assert.IsFalse(_cache.Get(CacheKeys.TagPrefix + "admintags:lang" + lang, out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.TagPrefix + "storycounts:lang" + lang + ":min1", out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.TagPrefix + "entity_counts:lang" + lang + ":min1", out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.TagPrefix + "storefront:lang" + lang, out string _));

            // Unrelated family survives a targeted purge.
            Assert.IsTrue(_cache.Get(settingsKey, out string kept));
            Assert.AreEqual("keep-me", kept);
        }

        [TestMethod]
        public void MenuInvalidation_DropsNavTreeActiveMenusAndAdminTree()
        {
            int lang = _rnd.Next(1, 100);
            Seed(CacheKeys.MenuPrefix + "navtree:lang" + lang, "v");
            Seed(CacheKeys.MenuPrefix + "navtree:lang" + lang + AsyncSuffix, "v");
            Seed(CacheKeys.MenuPrefix + "activemenus:lang" + lang, "v");
            Seed(CacheKeys.MenuPrefix + "admintree:True:lang" + lang, "v");

            Evict(MenuInvalidationPrefixes);

            Assert.IsFalse(_cache.Get(CacheKeys.MenuPrefix + "navtree:lang" + lang, out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.MenuPrefix + "navtree:lang" + lang + AsyncSuffix, out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.MenuPrefix + "activemenus:lang" + lang, out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.MenuPrefix + "admintree:True:lang" + lang, out string _));
        }

        [TestMethod]
        public void BrandInvalidation_DropsCategoryScopedBrandLists()
        {
            int lang = _rnd.Next(1, 100);
            int catId = _rnd.Next(100000, int.MaxValue - 1);
            string catScoped = CacheKeys.BrandPrefix + "list:cat" + catId + ":lang" + lang;

            Seed(catScoped, "v");
            Seed(CacheKeys.BrandList(lang), "v");

            Evict(BrandInvalidationPrefixes);

            Assert.IsFalse(_cache.Get(catScoped, out string _));
            Assert.IsFalse(_cache.Get(CacheKeys.BrandList(lang), out string _));
        }

        #endregion

        #region Detail caching round-trip

        [TestMethod]
        public async Task DetailGetOrAdd_PopulatesOnce_ThenServesFromCache()
        {
            int productId = _rnd.Next(100000, int.MaxValue - 1);
            var key = CacheKeys.ProductDetail(productId);
            int factoryCalls = 0;

            var first = await _cache.GetOrAddAsync(
                CacheKeys.ProductDetailAsync(productId),
                () => { Interlocked.Increment(ref factoryCalls); return Task.FromResult("dto-v1"); },
                CachePolicy.Absolute(60)).ConfigureAwait(false);

            var second = await _cache.GetOrAddAsync(
                CacheKeys.ProductDetailAsync(productId),
                () => { Interlocked.Increment(ref factoryCalls); return Task.FromResult("dto-v2"); },
                CachePolicy.Absolute(60)).ConfigureAwait(false);

            Assert.AreEqual("dto-v1", first);
            Assert.AreEqual("dto-v1", second);
            Assert.AreEqual(1, factoryCalls);

            // After a product mutation the detail family is evicted; next read repopulates.
            _cache.ClearByPrefix(CacheKeys.ProductDetailPrefix);
            var third = await _cache.GetOrAddAsync(
                CacheKeys.ProductDetailAsync(productId),
                () => { Interlocked.Increment(ref factoryCalls); return Task.FromResult("dto-v2"); },
                CachePolicy.Absolute(60)).ConfigureAwait(false);

            Assert.AreEqual("dto-v2", third);
            Assert.AreEqual(2, factoryCalls);
            Assert.IsFalse(_cache.Get(key, out string _), "sync detail entry was never seeded in this test");
        }

        [TestMethod]
        public void GetOrAdd_ConcurrentMisses_ExecuteFactoryOnce_SingleFlight()
        {
            var key = "product:list:singleflight:" + Guid.NewGuid().ToString("N");
            int calls = 0;
            using (var gate = new ManualResetEventSlim(false))
            {
                var worker = Task.Run(() =>
                    _cache.GetOrAdd(key, () => { Interlocked.Increment(ref calls); gate.Wait(2000); return 7; }, CachePolicy.Absolute(60)));

                Thread.Sleep(50); // let the worker claim the key first
                var racers = Task.WhenAll(
                    Task.Run(() => _cache.GetOrAdd(key, () => { Interlocked.Increment(ref calls); return 13; }, CachePolicy.Absolute(60))),
                    Task.Run(() => _cache.GetOrAdd(key, () => { Interlocked.Increment(ref calls); return 13; }, CachePolicy.Absolute(60))));

                gate.Set();
                Assert.AreEqual(7, worker.GetAwaiter().GetResult());
                CollectionAssert.AreEqual(new[] { 7, 7 }, racers.GetAwaiter().GetResult());
                Assert.AreEqual(1, calls, "single-flight must coalesce concurrent misses onto one factory call");
            }
        }

        #endregion

        private void Seed(string logicalKey, object value)
        {
            _cache.Set(logicalKey, value, CachePolicy.Absolute(120));
            Assert.IsTrue(_cache.Get(logicalKey, out object _), "seed failed for " + logicalKey);
        }

        private void Evict(string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                _cache.ClearByPrefix(prefix);
            }
        }
    }
}
