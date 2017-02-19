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
using System.Threading;
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
            mainPageModel.CurrentLanguage = CurrentLanguage;
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
        [OutputCache(CacheProfile = "Cache30Days")]
        public ActionResult TermsAndConditions()
        {
            var stringContent = SettingService.GetSettingObjectByKey(Settings.TermsAndConditions, CurrentLanguage);
            return View(stringContent);
        }
        [OutputCache(CacheProfile = "Cache30Days")]
        public ActionResult PrivacyPolicy()
        {
            var stringContent = SettingService.GetSettingObjectByKey(Settings.PrivacyPolicy, CurrentLanguage);
            return View(stringContent);
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
        [OutputCache(Duration = Settings.PartialViewOutputCachingDuration, VaryByParam = "none")]
        public ActionResult WebSiteLogo()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Settings.WebSiteLogo);
            return PartialView("_WebSiteLogo", webSiteLogo);
        }
        [OutputCache(Duration = Settings.PartialViewOutputCachingDuration, VaryByParam = "none")]
        public ActionResult WebSiteAddressInfo()
        {
            var WebSiteCompanyPhoneAndLocation = SettingService.GetSettingByKey("WebSiteCompanyPhoneAndLocation");
            var WebSiteCompanyEmailAddress = SettingService.GetSettingByKey("WebSiteCompanyEmailAddress");
            var resultList = new List<String>();
            resultList.Add(WebSiteCompanyPhoneAndLocation);
            resultList.Add(WebSiteCompanyEmailAddress);
            return PartialView("_WebSiteAddressInfo", resultList);
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
                s.Note = String.Format("CompanyName:{0} {4}Phone:{1} {4}Address:{2} {4}Message:{3} ",
                    contact.CompanyName,
                    contact.Phone,
                    contact.Address,
                    contact.Message, Environment.NewLine);
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
        public ActionResult SetLanguage(string name)
        {
            //  name = CultureHelper.GetImplementedCulture(name);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(name);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            Response.Cookies[CultureCookieName].Value = name;
            MemoryCacheProvider.ClearAll();
            return RedirectToAction("Index", "Home");
        }
    }
}