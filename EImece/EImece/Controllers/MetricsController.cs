using EImece.Domain.Observability.Metrics;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [Authorize(Roles = Domain.Constants.AdministratorRole)]
    public class MetricsController : Controller
    {
        private readonly IApplicationMetrics _metrics;

        public MetricsController(IApplicationMetrics metrics)
        {
            _metrics = metrics;
        }

        [HttpGet]
        [Route("metrics")]
        public ActionResult Index()
        {
            Response.ContentType = "application/json";
            return Content(JsonConvert.SerializeObject(_metrics.GetSnapshots(), Formatting.Indented), "application/json");
        }
    }
}
