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
using System.Web.UI;

namespace EImece.Controllers
{
    public class HomeController : BaseController
    {
        private static readonly Logger HomeLogger = LogManager.GetCurrentClassLogger();


        public ActionResult Index()
        {
            MainPageViewModel mainPageModel = MainPageImageService.GetMainPageViewModel(CurrentLanguage);
            ViewBag.Title = SettingService.GetSettingByKey("SiteIndexMetaTitle").ToStr();
            ViewBag.Description = SettingService.GetSettingByKey("SiteIndexMetaDescription").ToStr();
            ViewBag.Keywords = SettingService.GetSettingByKey("SiteIndexMetaKeywords").ToStr();

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
        [OutputCache(Duration = Settings.PartialViewOutputCachingDuration, VaryByParam = "none")]
        public ActionResult SocialMediaLinks()
        {
            var InstagramWebSiteLink = SettingService.GetSettingByKey("InstagramWebSiteLink");
            var TwitterWebSiteLink = SettingService.GetSettingByKey("TwitterWebSiteLink");
            var LinkedinWebSiteLink = SettingService.GetSettingByKey("LinkedinWebSiteLink");
            var GooglePlusWebSiteLink = SettingService.GetSettingByKey("GooglePlusWebSiteLink");
            var FacebookWebSiteLink = SettingService.GetSettingByKey("FacebookWebSiteLink");

            var resultList = new List<String>();
            resultList.Add(InstagramWebSiteLink);
            resultList.Add(TwitterWebSiteLink);
            resultList.Add(LinkedinWebSiteLink);
            resultList.Add(GooglePlusWebSiteLink);
            resultList.Add(FacebookWebSiteLink);

            return PartialView("_SocialMediaLinks", resultList);
        }
        [ChildActionOnly]
        [OutputCache(Duration = Settings.PartialViewOutputCachingDuration, VaryByParam = "none")]
        public ActionResult GoogleAnalyticsTrackingScript()
        {
            var GoogleAnalyticsTrackingScript = SettingService.GetSettingByKey("GoogleAnalyticsTrackingScript").ToStr();
            return Content(GoogleAnalyticsTrackingScript);
        }
        [OutputCache(Duration = Settings.PartialViewOutputCachingDuration, VaryByParam = "none")]
        public ActionResult Menu()
        {
            var menus = MenuService.BuildTree(true, CurrentLanguage);
            return PartialView("_Navigation", menus);
        }
        [OutputCache(Duration = Settings.PartialViewOutputCachingDuration, VaryByParam = "none")]
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
                s.Note = String.Format("{0} {1} {2} {3} ", contact.CompanyName, contact.Phone, contact.Message, contact.Address);
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

            try
            {
                EmailSender.SendEmailContactingUs(contact);

            }
            catch (Exception ex)
            {
                HomeLogger.Error(ex, "Exception Message:" + ex.Message);
            }


          

            return View("_pThankYouForContactingUs", contact);

        }
    }
}