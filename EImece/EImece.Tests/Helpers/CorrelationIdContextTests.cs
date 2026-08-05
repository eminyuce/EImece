using EImece.Domain.Observability.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class CorrelationIdContextTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            HttpContext.Current = null;
        }

        [TestMethod]
        public void Ensure_GeneratesCorrelationId_WhenMissing()
        {
            HttpContext.Current = CreateHttpContext();
            var id = CorrelationIdContext.Ensure();
            Assert.IsFalse(string.IsNullOrWhiteSpace(id));
            Assert.AreEqual(id, CorrelationIdContext.Current);
        }

        [TestMethod]
        public void Ensure_ReusesExistingCorrelationId()
        {
            HttpContext.Current = CreateHttpContext();
            CorrelationIdContext.Current = "fixed-correlation-id";
            Assert.AreEqual("fixed-correlation-id", CorrelationIdContext.Ensure());
        }

        [TestMethod]
        public void TryGetParentContext_ParsesW3CTraceParent()
        {
            HttpContext.Current = CreateHttpContext();
            CorrelationIdContext.TraceParent = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";

            ActivityContext parent;
            Assert.IsTrue(CorrelationIdContext.TryGetParentContext(out parent));
            Assert.AreEqual("4bf92f3577b34da6a3ce929d0e0e4736", parent.TraceId.ToString());
            Assert.AreEqual("00f067aa0ba902b7", parent.SpanId.ToString());
        }

        [TestMethod]
        public void TryGetParentContext_ReturnsFalse_WhenMissing()
        {
            HttpContext.Current = CreateHttpContext();
            ActivityContext parent;
            Assert.IsFalse(CorrelationIdContext.TryGetParentContext(out parent));
        }

        private static HttpContext CreateHttpContext()
        {
            var request = new HttpRequest("", "http://localhost/", "");
            var response = new HttpResponse(new StringWriter());
            return new HttpContext(request, response);
        }
    }
}
