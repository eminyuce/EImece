using EImece.Domain.Observability.HealthChecks;
using Newtonsoft.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [AllowAnonymous]
    public class HealthController : Controller
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthController(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        /**
         *  icacls "C:\inetpub\wwwroot\Eimece\media\images" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
         *  run it in command prompt to give permission to the app pool identity to write to the images folder as admin 
         */

        [HttpGet]
        [Route("health")]
        [Route("healthz")]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var response = await _healthCheckService.GetHealthAsync(cancellationToken).ConfigureAwait(false);
            var statusCode = response.Status == "UP" ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable;
            Response.StatusCode = (int)statusCode;
            Response.ContentType = "application/json";
            return Content(JsonConvert.SerializeObject(response, Formatting.Indented), "application/json");
        }
    }
}
