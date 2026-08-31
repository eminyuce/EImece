using EImece.Domain.Caching;
using EImece.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class MemoryCacheProviderTests
    {
        private MemoryCacheProvider _cache;
        private string _prefix;

        [TestInitialize]
        public void Init()
        {
            _cache = new MemoryCacheProvider(TestNullLoggers.Create<MemoryCacheProvider>());
            _prefix = "test:perf:" + Guid.NewGuid().ToString("N") + ":";
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cache.ClearByPrefix(_prefix);
        }

        [TestMethod]
        public void GetOrAdd_Absolute_ReturnsCachedValueOnSecondCall()
        {
            var key = _prefix + "absolute";
            var calls = 0;

            var first = _cache.GetOrAdd(key, () => { calls++; return 42; }, CachePolicy.Absolute(60));
            var second = _cache.GetOrAdd(key, () => { calls++; return 99; }, CachePolicy.Absolute(60));

            Assert.AreEqual(42, first);
            Assert.AreEqual(42, second);
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        public void ClearByPrefix_RemovesMatchingKeysOnly()
        {
            var keepKey = _prefix + "keep";
            var dropKey = _prefix + "list:drop";

            _cache.Set(keepKey, "keep-value", CachePolicy.Absolute(60));
            _cache.Set(dropKey, "drop-value", CachePolicy.Absolute(60));

            var removed = _cache.ClearByPrefix(_prefix + "list:");
            Assert.IsTrue(removed >= 1);

            Assert.IsTrue(_cache.Get(keepKey, out string keep));
            Assert.AreEqual("keep-value", keep);
            Assert.IsFalse(_cache.Get(dropKey, out string _));
        }

        [TestMethod]
        public void GetOrAdd_Sliding_RemainsAvailableWhileTouched()
        {
            var key = _prefix + "sliding";
            var value = _cache.GetOrAdd(key, () => "warm", CachePolicy.Sliding(2));
            Assert.AreEqual("warm", value);

            Thread.Sleep(800);
            Assert.IsTrue(_cache.Get(key, out string again));
            Assert.AreEqual("warm", again);
        }

        [TestMethod]
        public void Clear_RemovesExactLogicalKey()
        {
            var key = _prefix + "exact";
            _cache.Set(key, 7, CachePolicy.Absolute(60));
            _cache.Clear(key);
            Assert.IsFalse(_cache.Get(key, out int _));
        }
    }
}
