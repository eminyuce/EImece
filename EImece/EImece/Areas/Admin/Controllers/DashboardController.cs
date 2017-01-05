using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SearchContent(String search)
        {
            ViewBag.SearchKey = search;
            return View();
        }
        public ActionResult ClearCache()
        {
            MemoryCacheProvider.ClearAll();
            var urlReferrer = Request.UrlReferrer;
            if (urlReferrer != null)
            {
                return Redirect(urlReferrer.ToStr());
            }
            else
            {
                return RedirectToAction("Index");
            }
   
        }
    }
}