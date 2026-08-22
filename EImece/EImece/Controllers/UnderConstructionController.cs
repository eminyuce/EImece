using EImece.Domain;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using NLog;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class UnderConstructionController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public ISettingService SettingService { get; set; }

        public UnderConstructionController()
        {
        }

        public UnderConstructionController(ISettingService settingService)
        {
            SettingService = settingService;
        }

        // GET: UnderConstruction
        public async Task<ActionResult> Index()
        {
            Logger.Info("Entering Index action.");

            bool isSiteUnderConstruction = false;
            ISettingService settingService = SettingService;
            if (settingService == null)
            {
                try
                {
                    settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                }
                catch
                {
                    settingService = null;
                }
            }

            if (settingService != null)
            {
                var settingVal = await settingService.GetSettingByKeyAsync(Constants.IsSiteUnderConstruction);
                isSiteUnderConstruction = settingVal.ToBool(false);
            }

            Logger.Info($"Checking site status: IsSiteUnderConstruction = {isSiteUnderConstruction}");

            if (isSiteUnderConstruction)
            {
                Logger.Info("Site is under construction. Setting response to ServiceUnavailable (503).");
                var response = HttpContext?.Response;
                if (response != null)
                {
                    response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    response.TrySkipIisCustomErrors = true;
                }
                Logger.Info("Returning Index view with 503 status.");
                return await Task.FromResult(View());
            }
            else
            {
                Logger.Info("Site is not under construction. Redirecting to Home Index.");
                return await Task.FromResult(RedirectToAction("Index", "Home"));
            }
        }
    }
}