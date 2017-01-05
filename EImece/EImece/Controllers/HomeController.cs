using EImece.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Menu()
        {
            var menus = MenuService.BuildTree();
            return PartialView("_Navigation", menus);
        }
        public ActionResult ProductTree()
        {
            var tree = ProductCategoryService.BuildTree(true,Settings.MainLanguage);
            return PartialView("_ProductCategoryTree", tree);
        }
    }
}