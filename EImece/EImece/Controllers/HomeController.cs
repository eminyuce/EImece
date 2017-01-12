using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
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
            MainPageViewModel mainPageModel = MainPageImageService.GetMainPageViewModel(Settings.MainLanguage);
            return View(mainPageModel);
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
        public ActionResult AddSubscriber(Subscriber subscriber)
        {
            SubsciberService.SaveOrEditEntity(subscriber);
            return RedirectToAction("ThanksForSubscription",new { id= subscriber.Id });
        }
        public ActionResult ThanksForSubscription(int id)
        {
            var s = SubsciberService.GetSingle(id);
            return View(s);
        }
        public ActionResult Menu()
        {
            var menus = MenuService.BuildTree(true, Settings.MainLanguage);
            return PartialView("_Navigation", menus);
        }
        public ActionResult ProductTree()
        {
            var tree = ProductCategoryService.BuildTree(true,Settings.MainLanguage);
            return PartialView("_ProductCategoryTree", tree);
        }
    }
}