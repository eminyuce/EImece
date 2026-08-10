using EImece.Domain;
using EImece.Domain.Observability.HealthChecks;
using Newtonsoft.Json;
using System;
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
        /*
         * One writable root for uploads + NLog files (media/images and media/logs).
         * Run elevated after publish:
         *
         *   mkdir "C:\inetpub\wwwroot\Eimece\media\images" 2>nul
         *   mkdir "C:\inetpub\wwwroot\Eimece\media\logs" 2>nul
         *   icacls "C:\inetpub\wwwroot\Eimece\media" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
         *
         * See docs/IIS_APP_POOL_PERMISSIONS.md
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

            // Anonymous callers only get aggregate status (no dependency error details).
            var isAdmin = User?.Identity != null
                && User.Identity.IsAuthenticated
                && User.IsInRole(Constants.AdministratorRole);

            if (isAdmin)
            {
                return Content(JsonConvert.SerializeObject(response, Formatting.Indented), "application/json");
            }

            var publicPayload = new { status = response.Status };
            return Content(JsonConvert.SerializeObject(publicPayload), "application/json");
        }
    }
}
