using EImece.Filters;
using System.Web.Mvc;

namespace EImece.Controllers
{
    /// <summary>
    /// Example controller showing how to use the opt-in [Timed] business metric.
    /// Overall HTTP request duration is already tracked automatically by
    /// OpenTelemetry.Instrumentation.AspNet; [Timed] adds a separate histogram
    /// with your chosen business name (visible in OTLP / Azure Monitor).
    /// </summary>
    public class ConversationsController : Controller
    {
        // Business-oriented histogram: service.conversations.getConversations (ms)
        // Use any dot-separated name you like; keep it low-cardinality (no IDs).
        [Timed("service.conversations.getConversations", "Time taken to get conversations")]
        public ActionResult GetConversations()
        {
            // action logic — e.g. fetch from service / DB
            return View();
        }

        // Example: class-level [Timed] applies to every action in the controller.
        // Uncomment to time all actions with one metric:
        // [Timed("service.conversations.all", "All conversation actions")]
        // public class ConversationsController : Controller { ... }
    }
}
