using EImece.App_Start;
using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Metrics;
using EImece.Domain.Observability.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class PerfStatsTests
    {
        [TestInitialize]
        public void Setup()
        {
            PerfStats.RetentionHoursProvider = null;
            PerfStats.Clear();
        }

        [TestCleanup]
        public void Cleanup()
        {
            PerfStats.RetentionHoursProvider = null;
            PerfStats.Clear();
        }

        [TestMethod]
        public void Record_AggregatesCountSumMinMaxAvgLast()
        {
            PerfStats.Record("service.products.search", 10.0);
            PerfStats.Record("service.products.search", 20.0);
            PerfStats.Record("service.products.search", 30.0);

            var snapshots = PerfStats.Snapshot();
            var item = snapshots.FirstOrDefault(s => s.Name == "service.products.search");

            Assert.IsNotNull(item);
            Assert.AreEqual(3, item.Count);
            Assert.AreEqual(60.0, item.SumMs, 0.001);
            Assert.AreEqual(20.0, item.AvgMs, 0.001);
            Assert.AreEqual(10.0, item.MinMs, 0.001);
            Assert.AreEqual(30.0, item.MaxMs, 0.001);
            Assert.AreEqual(30.0, item.LastMs, 0.001);
            Assert.IsTrue((DateTime.UtcNow - item.LastUtc).TotalSeconds < 5);
        }

        [TestMethod]
        public void Snapshot_ReturnsSortedByAvgMsDescending()
        {
            PerfStats.Record("fast.metric", 5.0);
            PerfStats.Record("slow.metric", 100.0);
            PerfStats.Record("medium.metric", 50.0);

            var snapshots = PerfStats.Snapshot();

            Assert.AreEqual(3, snapshots.Count);
            Assert.AreEqual("slow.metric", snapshots[0].Name);
            Assert.AreEqual("medium.metric", snapshots[1].Name);
            Assert.AreEqual("fast.metric", snapshots[2].Name);
        }

        [TestMethod]
        public void Clear_ClearsOnlyPerfStats()
        {
            PerfStats.Record("test.metric", 15.0);
            Assert.AreEqual(1, PerfStats.Snapshot().Count);

            PerfStats.Clear();
            Assert.AreEqual(0, PerfStats.Snapshot().Count);
        }

        [TestMethod]
        public void ConcurrentRecord_IsThreadSafe()
        {
            const int threads = 8;
            const int iterationsPerThread = 500;

            Parallel.For(0, threads, _ =>
            {
                for (int i = 0; i < iterationsPerThread; i++)
                {
                    PerfStats.Record("service.concurrent.test", 10.0);
                }
            });

            var snapshots = PerfStats.Snapshot();
            var item = snapshots.FirstOrDefault(s => s.Name == "service.concurrent.test");

            Assert.IsNotNull(item);
            Assert.AreEqual(threads * iterationsPerThread, item.Count);
            Assert.AreEqual(threads * iterationsPerThread * 10.0, item.SumMs, 0.001);
            Assert.AreEqual(10.0, item.AvgMs, 0.001);
        }

        [TestMethod]
        public void Record_IgnoresNullOrEmptyName()
        {
            PerfStats.Record(null, 10.0);
            PerfStats.Record("", 10.0);
            PerfStats.Record("   ", 10.0);

            var snapshots = PerfStats.Snapshot();
            Assert.AreEqual(0, snapshots.Count);
        }

        [TestMethod]
        public void TimedAttribute_ProxyFactory_RecordsIntoPerfStats()
        {
            var service = new DummyTimedService();
            var proxy = ProxyFactory.Create<DummyTimedService>(service);

            proxy.ExecuteFast();
            proxy.ExecuteSlow();

            var snapshots = PerfStats.Snapshot();
            var fast = snapshots.FirstOrDefault(s => s.Name == "service.dummy.fast");
            var slow = snapshots.FirstOrDefault(s => s.Name == "service.dummy.slow");

            Assert.IsNotNull(fast);
            Assert.IsNotNull(slow);
            Assert.AreEqual(1, fast.Count);
            Assert.AreEqual(1, slow.Count);
            Assert.IsTrue(slow.AvgMs >= fast.AvgMs);
        }

        [TestMethod]
        public void Retention_SetToZero_DisablesCollection()
        {
            PerfStats.RetentionHoursProvider = () => 0;

            Assert.IsFalse(PerfStats.IsEnabled);
            Assert.AreEqual(0, PerfStats.GetRetentionHours());

            PerfStats.Record("service.disabled.test", 10.0);

            var snapshots = PerfStats.Snapshot();
            Assert.AreEqual(0, snapshots.Count);
        }

        [TestMethod]
        public void Retention_SetToPositiveHours_EnablesCollectionWithCustomWindow()
        {
            PerfStats.RetentionHoursProvider = () => 4;

            Assert.IsTrue(PerfStats.IsEnabled);
            Assert.AreEqual(4, PerfStats.GetRetentionHours());

            PerfStats.Record("service.custom_hours.test", 42.0);

            var snapshots = PerfStats.Snapshot();
            Assert.AreEqual(1, snapshots.Count);
            Assert.AreEqual(42.0, snapshots[0].AvgMs, 0.001);
        }

        [TestMethod]
        public void DependencyInjection_InterfaceService_RecordsBothPerfStatsAndMetrics()
        {
            var services = new ServiceCollection();
            var metrics = new ApplicationMetrics();
            var options = new ObservabilityOptions { EnableMetrics = true, EnableServiceMethodMetrics = true };
            services.AddSingleton<IApplicationMetrics>(metrics);
            services.AddSingleton(options);
            services.AddScopedWithProps<ITestOrderService, TestOrderService>();

            using (var sp = services.BuildServiceProvider())
            {
                var service = sp.GetRequiredService<ITestOrderService>();
                service.PlaceOrder();
                var result = service.PlaceOrderAsync().GetAwaiter().GetResult();
                Assert.AreEqual(123, result);
            }

            var snapshots = PerfStats.Snapshot();
            var syncStat = snapshots.FirstOrDefault(s => s.Name == "service.orders.place");
            var asyncStat = snapshots.FirstOrDefault(s => s.Name == "service.orders.place_async");

            Assert.IsNotNull(syncStat, "service.orders.place should be recorded in PerfStats");
            Assert.IsNotNull(asyncStat, "service.orders.place_async should be recorded in PerfStats");
            Assert.AreEqual(1, syncStat.Count);
            Assert.AreEqual(1, asyncStat.Count);
        }

        [TestMethod]
        public void DependencyInjection_ConcreteService_RecordsPerfStats()
        {
            var services = new ServiceCollection();
            services.AddScopedWithProps<TestConcreteService>();

            using (var sp = services.BuildServiceProvider())
            {
                var service = sp.GetRequiredService<TestConcreteService>();
                service.DoWork();
            }

            var snapshots = PerfStats.Snapshot();
            var stat = snapshots.FirstOrDefault(s => s.Name == "service.concrete.action");
            Assert.IsNotNull(stat, "service.concrete.action should be recorded in PerfStats");
            Assert.AreEqual(1, stat.Count);
        }
    }

    public class DummyTimedService
    {
        [Timed("service.dummy.fast")]
        public virtual void ExecuteFast()
        {
            // no-op
        }

        [Timed("service.dummy.slow")]
        public virtual void ExecuteSlow()
        {
            System.Threading.Thread.Sleep(5);
        }
    }

    public interface ITestOrderService
    {
        void PlaceOrder();
        Task<int> PlaceOrderAsync();
    }

    public class TestOrderService : ITestOrderService
    {
        [Timed("service.orders.place")]
        public virtual void PlaceOrder()
        {
        }

        [Timed("service.orders.place_async")]
        public virtual async Task<int> PlaceOrderAsync()
        {
            await Task.Yield();
            return 123;
        }
    }

    public class TestConcreteService
    {
        [Timed("service.concrete.action")]
        public virtual void DoWork()
        {
        }
    }
}
