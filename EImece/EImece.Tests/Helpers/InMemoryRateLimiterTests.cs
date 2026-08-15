using EImece.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class InMemoryRateLimiterTests
    {
        [TestInitialize]
        [TestCleanup]
        public void Cleanup()
        {
            InMemoryRateLimiter.Reset();
        }

        [TestMethod]
        public void Check_AllowsRequestsUnderLimit()
        {
            string key = "test:127.0.0.1";
            int limit = 3;
            TimeSpan window = TimeSpan.FromMinutes(1);

            var r1 = InMemoryRateLimiter.Check(key, limit, window);
            var r2 = InMemoryRateLimiter.Check(key, limit, window);
            var r3 = InMemoryRateLimiter.Check(key, limit, window);

            Assert.IsTrue(r1.IsAllowed);
            Assert.AreEqual(2, r1.Remaining);
            Assert.IsTrue(r2.IsAllowed);
            Assert.AreEqual(1, r2.Remaining);
            Assert.IsTrue(r3.IsAllowed);
            Assert.AreEqual(0, r3.Remaining);
        }

        [TestMethod]
        public void Check_BlocksRequestsWhenLimitExceeded()
        {
            string key = "login:10.0.0.1";
            int limit = 2;
            TimeSpan window = TimeSpan.FromMinutes(5);

            InMemoryRateLimiter.Check(key, limit, window);
            InMemoryRateLimiter.Check(key, limit, window);
            var blockedResult = InMemoryRateLimiter.Check(key, limit, window);

            Assert.IsFalse(blockedResult.IsAllowed);
            Assert.AreEqual(0, blockedResult.Remaining);
            Assert.IsTrue(blockedResult.RetryAfterSeconds > 0);
            Assert.IsTrue(blockedResult.RetryAfterSeconds <= 300);
        }

        [TestMethod]
        public void Check_IsolatesDifferentKeys()
        {
            string keyA = "search:192.168.1.10";
            string keyB = "search:192.168.1.20";
            int limit = 1;
            TimeSpan window = TimeSpan.FromMinutes(1);

            var resA1 = InMemoryRateLimiter.Check(keyA, limit, window);
            var resA2 = InMemoryRateLimiter.Check(keyA, limit, window);
            var resB1 = InMemoryRateLimiter.Check(keyB, limit, window);

            Assert.IsTrue(resA1.IsAllowed);
            Assert.IsFalse(resA2.IsAllowed);
            Assert.IsTrue(resB1.IsAllowed); // keyB should still be allowed
        }

        [TestMethod]
        public void Check_HandlesZeroOrNegativeLimitGracefully()
        {
            var res = InMemoryRateLimiter.Check("any:key", 0, TimeSpan.FromMinutes(1));
            Assert.IsTrue(res.IsAllowed);

            var resNeg = InMemoryRateLimiter.Check("any:key", -5, TimeSpan.FromMinutes(1));
            Assert.IsTrue(resNeg.IsAllowed);
        }

        [TestMethod]
        public void Check_ConcurrentRequests_RespectsLimit()
        {
            string key = "checkout:172.16.0.5";
            int limit = 10;
            TimeSpan window = TimeSpan.FromMinutes(1);
            int totalRequests = 50;
            int allowedCount = 0;
            int blockedCount = 0;

            Parallel.For(0, totalRequests, i =>
            {
                var res = InMemoryRateLimiter.Check(key, limit, window);
                if (res.IsAllowed)
                {
                    System.Threading.Interlocked.Increment(ref allowedCount);
                }
                else
                {
                    System.Threading.Interlocked.Increment(ref blockedCount);
                }
            });

            Assert.AreEqual(limit, allowedCount);
            Assert.AreEqual(totalRequests - limit, blockedCount);
        }
    }
}
