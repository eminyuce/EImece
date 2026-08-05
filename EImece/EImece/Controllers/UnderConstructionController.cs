using EImece.Domain;
using NLog;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class UnderConstructionController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // Added logger instance

        // GET: UnderConstruction
        public ActionResult Index()
        {
            Logger.Info("Entering Index action.");
            Logger.Info($"Checking site status: IsSiteUnderConstruction = {AppConfig.IsSiteUnderConstruction}");

            if (AppConfig.IsSiteUnderConstruction)
            {
                Logger.Info("Site is under construction. Setting response to ServiceUnavailable (503).");
                var response = HttpContext.Response;
                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.TrySkipIisCustomErrors = true;
                Logger.Info("Returning Index view with 503 status.");
                return View();
            }
            else
            {
                Logger.Info("Site is not under construction. Redirecting to Home Index.");
                return RedirectToAction("Index", "Home");
            }
        }

    }
}