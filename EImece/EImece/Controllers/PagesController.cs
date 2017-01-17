using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class PagesController : BaseController
    {
        // GET: Pages
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Detail(String id="")
        {
            var menuId = id.GetId();
            var page = MenuService.GetPageById(menuId);
            if (page.Menu.IsActive)
            {
                return View(page);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}