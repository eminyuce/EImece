using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [RoutePrefix(Constants.PagesControllerRoutingPrefix)]
    public class PagesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IMenuService MenuService { get; set; }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Detail(String id = "")
        {
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var menuId = id.GetId();
                var page = await MenuService.GetPageByIdAsync(menuId);
                if (page == null || page.Menu == null)
                {
                    Logger.Warn("Pages/Detail: menu not found for id '{0}' (parsed {1}).", id, menuId);
                    return RedirectToAction("NotFound", "Error");
                }

                ViewBag.SeoId = page.Menu.GetSeoUrl();
                if (page.Menu.IsActive)
                {
                    return View(page);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                return HandleUnexpectedError(ex, ex.Message + " id:" + id);
            }
        }
    }
}