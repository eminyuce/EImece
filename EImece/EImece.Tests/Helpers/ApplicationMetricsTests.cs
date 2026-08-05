using EImece.Domain.Observability.Metrics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
