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

            if (isSiteUnderConstruction)
            {
                Logger.Debug("Site is under construction. Returning ServiceUnavailable (503).");
                var response = HttpContext?.Response;
                if (response != null)
                {
                    response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    response.TrySkipIisCustomErrors = true;
                }
                var customHtml = settingService != null ? await settingService.GetSettingByKeyAsync(Constants.UnderConstructionHtml) : string.Empty;
                ViewBag.CustomHtml = customHtml;
                return await Task.FromResult(View());
            }

            return await Task.FromResult(RedirectToAction("Index", "Home"));
        }
    }
}