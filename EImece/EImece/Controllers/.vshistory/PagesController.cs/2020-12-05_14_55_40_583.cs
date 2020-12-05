using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class PagesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IMenuService MenuService { get; set; }

        // GET: Pages
        public ActionResult Index()
        {
            return View();
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Detail(String id = "")
        {
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var menuId = id.GetId();
                var page = MenuService.GetPageById(menuId);
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
                Logger.Error(ex, ex.Message + " id:" + id);
                return RedirectToAction("InternalServerError", "Error");
            }
        }
    }
}