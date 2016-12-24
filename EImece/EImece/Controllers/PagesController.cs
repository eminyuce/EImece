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
            var menuId = id.Split("-".ToCharArray()).Last().ToInt();
            var page = MenuService.GetSingle(menuId);
            return View(page);
        }
    }
}