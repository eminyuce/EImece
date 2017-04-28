using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Helpers.AttributeHelper;

namespace EImece.Controllers
{
    public class PagesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // GET: Pages
        public ActionResult Index()
        {
            return View();
        }
        [CustomOutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Detail(String id = "")
        {
            try
            {
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