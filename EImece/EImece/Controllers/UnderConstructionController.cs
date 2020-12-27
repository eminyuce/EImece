using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class UnderConstructionController : Controller
    {
        // GET: UnderConstruction
        public ActionResult Index()
        {
            if (AppConfig.IsSiteUnderConstruction)
            {
                var response = HttpContext.Response;
                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.TrySkipIisCustomErrors = true;
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult RefreshIpAddress()
        {
            OfflineHelper.OfflineData = new OfflineFileData(Server.MapPath(OfflineFileData.OfflineFilePath));
            return RedirectToAction("Index", "Home");
        }
    }
}