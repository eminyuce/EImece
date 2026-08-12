using EImece.Domain.Observability.Metrics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ApplicationMetricsTests
    {
        [TestMethod]
        public void RecordRequest_StoresSnapshotWithoutRawUrls()
        {
            var metrics = new ApplicationMetrics();
            metrics.RecordRequest("Products.Detail", 42, true, "GET", 200);

            var snapshots = metrics.GetSnapshots();
            Assert.IsTrue(snapshots.ContainsKey("request:Products.Detail"));
            Assert.AreEqual(1, snapshots["request:Products.Detail"].Count);
            Assert.AreEqual(0, snapshots["request:Products.Detail"].ErrorCount);
        }

        [TestMethod]
        public void RecordHttpCall_UsesNormalizedOperationKey()
        {
            var metrics = new ApplicationMetrics();
            metrics.RecordHttpCall("https://cdn.example.com/images/12345/photo.jpg?token=secret", "GET", 200, 15, 0);

            var snapshots = metrics.GetSnapshots();
            Assert.IsTrue(snapshots.ContainsKey("http:GET:200"));
            Assert.IsFalse(string.Join(",", snapshots.Keys).Contains("token=secret"));
            Assert.IsFalse(string.Join(",", snapshots.Keys).Contains("photo.jpg"));
        }

        [TestMethod]
        public void NormalizeRoute_ReplacesIdsAndDropsQuery()
        {
            var normalized = OpenTelemetryMetrics.NormalizeRoute("/products/42/details?x=1");
            Assert.AreEqual("products.{id}.details", normalized);
        }

        [TestMethod]
        public void RecordDatabaseQuery_TracksErrors()
        {
            var metrics = new ApplicationMetrics();
            metrics.RecordDatabaseQuery("SELECT", 10, false);

            var snapshot = metrics.GetSnapshots()["db:SELECT"];
            Assert.AreEqual(1, snapshot.Count);
            Assert.AreEqual(1, snapshot.ErrorCount);
        }

        [TestMethod]
        public void RecordMethod_ComputesP50P75P90P95P99()
        {
            var metrics = new ApplicationMetrics(sampleCapacity: 256);

            // 100 samples: 1..100 ms → nearest-rank percentiles
            for (var i = 1; i <= 100; i++)
            {
                metrics.RecordMethod("service", "IProductService", "GetSingle", i, true);
            }

            var snapshot = metrics.GetSnapshots()["service:IProductService.GetSingle"];
            Assert.AreEqual(100, snapshot.Count);
            Assert.AreEqual(1, snapshot.MinDurationMs);
            Assert.AreEqual(100, snapshot.MaxDurationMs);
            Assert.AreEqual(50, snapshot.P50DurationMs);
            Assert.AreEqual(75, snapshot.P75DurationMs);
            Assert.AreEqual(90, snapshot.P90DurationMs);
            Assert.AreEqual(95, snapshot.P95DurationMs);
            Assert.AreEqual(99, snapshot.P99DurationMs);
            Assert.AreEqual(50.5d, snapshot.AverageDurationMs, 0.01d);
        }

        [TestMethod]
        public void LatencyPercentiles_NearestRank_EmptyAndEdges()
        {
            Assert.AreEqual(0, LatencyPercentiles.NearestRank(new long[0], 0.95));
            Assert.AreEqual(5, LatencyPercentiles.NearestRank(new long[] { 5 }, 0.99));
            Assert.AreEqual(1, LatencyPercentiles.NearestRank(new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0.10));
            Assert.AreEqual(10, LatencyPercentiles.NearestRank(new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 1.0));
        }

        [TestMethod]
        public void RingBuffer_CapsSampleWindowButKeepsLifetimeCount()
        {
            var metrics = new ApplicationMetrics(sampleCapacity: 32);
            for (var i = 0; i < 100; i++)
            {
                metrics.RecordMethod("controller", "Home", "Index", i, true);
            }

            var snapshot = metrics.GetSnapshots()["controller:Home.Index"];
            Assert.AreEqual(100, snapshot.Count);
            Assert.AreEqual(32, snapshot.SampleWindowSize);
            // Window holds the most recent 32 samples (68..99) → P99 near the top of that window
            Assert.IsGreaterThanOrEqualTo(90, snapshot.P99DurationMs);
        }

        [TestMethod]
        public void MeasuredServiceProxy_RecordsSyncAndAsyncDurations()
        {
            var metrics = new ApplicationMetrics();
            IProbeService probe = new ProbeService();
            var proxied = MeasuredServiceProxy.Create(probe, metrics);

            Assert.AreEqual(42, proxied.Add(40, 2));
            Assert.AreEqual(7, proxied.AddAsync(3, 4).GetAwaiter().GetResult());

            try
            {
                proxied.Fail();
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // expected
            }

            var snapshots = metrics.GetSnapshots();
            Assert.IsTrue(snapshots.ContainsKey("service:IProbeService.Add"));
            Assert.IsTrue(snapshots.ContainsKey("service:IProbeService.AddAsync"));
            Assert.IsTrue(snapshots.ContainsKey("service:IProbeService.Fail"));
            Assert.AreEqual(1, snapshots["service:IProbeService.Fail"].ErrorCount);
            Assert.AreEqual(0, snapshots["service:IProbeService.Add"].ErrorCount);
        }

        [TestMethod]
        public void MeasuredServiceProxy_DoesNotWrapNullMetrics()
        {
            IProbeService probe = new ProbeService();
            var same = MeasuredServiceProxy.Create(probe, null);
            Assert.AreSame(probe, same);
        }

        public interface IProbeService
        {
            int Add(int a, int b);

            Task<int> AddAsync(int a, int b);

            void Fail();
        }

        private sealed class ProbeService : IProbeService
        {
            public int Add(int a, int b)
            {
                return a + b;
            }

            public Task<int> AddAsync(int a, int b)
            {
                return Task.FromResult(a + b);
            }

            public void Fail()
            {
                throw new InvalidOperationException("boom");
            }
        }
    }
}
