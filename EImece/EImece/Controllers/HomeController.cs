using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.FrontModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class HomeController : BaseController
    {
        private static readonly Logger HomeLogger = LogManager.GetCurrentClassLogger();
        public ActionResult Index()
        {
            MainPageViewModel mainPageModel = MainPageImageService.GetMainPageViewModel(CurrentLanguage);
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
            return RedirectToAction("ThanksForSubscription", new { id = subscriber.Id });
        }
        public ActionResult ThanksForSubscription(int id)
        {
            var s = SubsciberService.GetSingle(id);
            return View(s);
        }
        public ActionResult Menu()
        {
            var menus = MenuService.BuildTree(true, CurrentLanguage);
            return PartialView("_Navigation", menus);
        }
        public ActionResult ProductTree()
        {
            var tree = ProductCategoryService.BuildTree(true, CurrentLanguage);
            return PartialView("_ProductCategoryTree", tree);
        }
        public ActionResult SendContactUs(ContactUsFormViewModel contact)
        {

            try
            {
                var s = new Subscriber();
                s.Email = contact.Email.ToStr();
                s.CreatedDate = DateTime.Now;
                s.IsActive = true;
                s.Name = contact.Name.ToStr();
                s.UpdatedDate = DateTime.Now;
                s.Position = 1;
                s.EntityHash = "";
                s.Lang = CurrentLanguage;
                SubsciberService.SaveOrEditEntity(s);

            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                HomeLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception ex)
            {
                HomeLogger.Error(ex, "Exception Message:" + ex.Message);
            }
       

            EmailSender.SendEmailContactingUs(contact);
            return View("_pThankYouForContactingUs", contact);

        }
    }
}