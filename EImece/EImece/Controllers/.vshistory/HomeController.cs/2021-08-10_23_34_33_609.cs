﻿using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class HomeController : BaseController
    {
        private const string CaptchaContactUsLogin = "CaptchaContactUsLogin";
        private static readonly Logger HomeLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IEimeceCacheProvider MemoryCacheProvider { get; set; }

        [Inject]
        public IEmailSender EmailSender { get; set; }

        [Inject]
        public ISubscriberService SubsciberService { get; set; }

        [Inject]
        public IMainPageImageService MainPageImageService { get; set; }

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }

        [Inject]
        public IMenuService MenuService { get; set; }

        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }

        [Inject]
        public RazorEngineHelper RazorEngineHelper { get; set; }

        [Inject]
        public IProductService ProductService { get; set; }

        [CustomOutputCache(CacheProfile = Constants.Cache1Hour)]
        public ActionResult Index()
        {
            MainPageViewModel mainPageModel = MainPageImageService.GetMainPageViewModel(CurrentLanguage);
            mainPageModel.CurrentLanguage = CurrentLanguage;
            ViewBag.Title = SettingService.GetSettingByKey(Constants.SiteIndexMetaTitle, CurrentLanguage).ToStr();
            ViewBag.Description = SettingService.GetSettingByKey(Constants.SiteIndexMetaDescription, CurrentLanguage).ToStr();
            ViewBag.Keywords = SettingService.GetSettingByKey(Constants.SiteIndexMetaKeywords, CurrentLanguage).ToStr();

            return View(mainPageModel);
        }
        public ActionResult TestPage()
        {
            var baseurl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            ViewBag.GetRawUrl = GetRawUrl(this.Request);
            return View();
        }
        
        [HttpPost]
        public ActionResult AddSubscriber(Subscriber subscriber)
        {
            var emailChecker = new EmailAddressAttribute();
            if (subscriber == null
                || string.IsNullOrEmpty(subscriber.Email.ToStr().Trim())
                || !emailChecker.IsValid(subscriber.Email.ToStr().Trim()))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                //validate the captcha through the session variable stored from GetCaptcha
                subscriber.Name = subscriber.Email;
                subscriber.IsActive = true;
                SubsciberService.SaveOrEditEntity(subscriber);
                return RedirectToAction("ThanksForSubscription", new { id = subscriber.Id });
            }
        }

        public ActionResult ThanksForSubscription(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var s = SubsciberService.GetSingle(id.Value);
            return View(s);
        }

        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        public ActionResult SocialMediaLinks()
        {
            var resultList = new Dictionary<String, String>();
            resultList.Add(Constants.InstagramWebSiteLink, SettingService.GetSettingByKey(Constants.InstagramWebSiteLink));
            resultList.Add(Constants.LinkedinWebSiteLink, SettingService.GetSettingByKey(Constants.LinkedinWebSiteLink));
            resultList.Add(Constants.YotubeWebSiteLink, SettingService.GetSettingByKey(Constants.YotubeWebSiteLink));
            resultList.Add(Constants.FacebookWebSiteLink, SettingService.GetSettingByKey(Constants.FacebookWebSiteLink));
            resultList.Add(Constants.TwitterWebSiteLink, SettingService.GetSettingByKey(Constants.TwitterWebSiteLink));
            resultList.Add(Constants.PinterestWebSiteLink, SettingService.GetSettingByKey(Constants.PinterestWebSiteLink));

            return PartialView("_SocialMediaLinks", resultList);
        }

        [ChildActionOnly]
        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        public ActionResult GoogleAnalyticsTrackingScript()
        {
            var GoogleAnalyticsTrackingScript = SettingService.GetSettingByKey(Constants.GoogleAnalyticsTrackingScript).ToStr();
            return Content(GoogleAnalyticsTrackingScript);
        }

        [ChildActionOnly]
        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        public ActionResult WhatsAppCommunicationScript()
        {
            return Content(SettingService.GetSettingByKey(Constants.WhatsAppCommunicationScript).ToStr());
        }

        public ActionResult Languages()
        {
            List<SelectListItem> listItems = EnumHelper.ToSelectList3("Language");
            return PartialView("_Languages", listItems);
        }

        public ActionResult GetHtmlLangCode()
        {
            switch ((EImeceLanguage)CurrentLanguage)
            {
                case EImeceLanguage.Turkish:
                    return Content("tr");

                case EImeceLanguage.English:
                    return Content("en");

                case EImeceLanguage.Russian:
                    return Content("ru");

                case EImeceLanguage.German:
                    return Content("de");
            }
            return Content("tr");
        }

        [ChildActionOnly]
        public ActionResult Navigation()
        {
            var menus = MenuService.BuildTree(true, CurrentLanguage);
            var tree = ProductCategoryService.BuildNavigation(true, CurrentLanguage);
            return PartialView("_Navigation", new NavigationModel(menus, tree));
        }

        [ChildActionOnly]
        public ActionResult ProductCategoryTree()
        {
            var tree = ProductCategoryService.BuildTree(true, CurrentLanguage);
            return PartialView("_ProductCategoryTree", tree);
        }

        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        [ChildActionOnly]
        public ActionResult WebSiteLogo()
        {
            var item = new SettingLayoutViewModel();
            item.WebSiteLogo = SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
            item.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName);
            return PartialView("_WebSiteLogo", item);
        }

        [ChildActionOnly]
        public ActionResult Footer()
        {
            var footerViewModel = MainPageImageService.GetFooterViewModel(CurrentLanguage);
            return PartialView("_Footer", footerViewModel);
        }

        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        public ActionResult GetCompanyName()
        {
            string companyName = SettingService.GetSettingByKey(Constants.CompanyName);
            return Content(companyName);
        }

        public ActionResult WebSiteAddressInfo(bool isMobilePage = false)
        {
            var item = new SettingLayoutViewModel();
            item.isMobilePage = isMobilePage;
            item.WebSiteCompanyPhoneAndLocation = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyPhoneAndLocation);
            item.WebSiteCompanyEmailAddress = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyEmailAddress);
            return PartialView("_WebSiteAddressInfo", item);
        }

        [HttpPost]
        public ActionResult SendContactUs(ContactUsFormViewModel contact)
        {
            if (contact == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Session[CaptchaContactUsLogin] == null || !Session[CaptchaContactUsLogin].ToString().Equals(contact.Captcha, StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("Captcha", Resource.ContactUsWrongSumForSecurityQuestion);
                ModelState.AddModelError("", Resource.ContactUsWrongSumForSecurityQuestion);
                if (contact.ItemType == EImeceItemType.Product)
                {
                    var product = ProductService.GetProductDetailViewModelById(contact.ItemId);
                    product.Contact = contact;
                    return View("../Products/Detail", product);
                }
                else if (contact.ItemType == EImeceItemType.Menu)
                {
                    var page = MenuService.GetPageById(contact.ItemId);
                    page.Contact = contact;
                    return View("../Pages/Detail", page);
                }
                return View("_ContactUsFormViewModel", contact);
            }
            else if (!validateContactUsFormViewModel(contact))
            {
                return View("_ContactUsFormViewModel", contact);
            }
            else
            {
                try
                {
                    saveSubsciber(contact);
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
                    if (contact.ItemType == EImeceItemType.Product)
                    {
                        // Sending email.
                        RazorEngineHelper.SendContactUsAboutProductDetailEmail(contact);
                    }
                    else
                    {
                        RazorEngineHelper.SendContactUsForCommunication(contact);
                    }
                }
                catch (Exception ex)
                {
                    HomeLogger.Error(ex, "Exception Message:" + ex.Message);
                }
            }

            return View("_pThankYouForContactingUs", contact);
        }

        private bool validateContactUsFormViewModel(ContactUsFormViewModel contact)
        {
            bool result = true;
            if (string.IsNullOrEmpty(contact.Email))
            {
                result = false;
                ModelState.AddModelError("Email", Resource.EmailRequired);
            }

            if (string.IsNullOrEmpty(contact.Name))
            {
                result = false;
                ModelState.AddModelError("Name", Resource.MandatoryField);
            }

            if (string.IsNullOrEmpty(contact.Message))
            {
                result = false;
                ModelState.AddModelError("Message", Resource.ContactUsMessageErrorMessage);
            }

            return result;
        }

        private void saveSubsciber(ContactUsFormViewModel contact)
        {
            var s = new Subscriber();
            s.Email = contact.Email.ToStr();
            s.CreatedDate = DateTime.Now;
            s.IsActive = true;
            s.Name = contact.Name.ToStr();
            s.UpdatedDate = DateTime.Now;
            s.Position = 1;
            s.Lang = CurrentLanguage;
            s.Note = string.Format("{0} {4} {1} {4} {2} {4} {3} ",
                contact.CompanyName,
                contact.Phone,
                contact.Address,
                contact.Message, Environment.NewLine);
            SubsciberService.SaveOrEditEntity(s);
        }

        public ActionResult Language(string id)
        {
            EImeceLanguage selectedLanguage = (EImeceLanguage)id.ToInt();
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);

            CreateLanguageCookie(selectedLanguage, Constants.CultureCookieName);
            MemoryCacheProvider.ClearAll();

            Response.Cookies.Remove("Language");

            var languageCookie = System.Web.HttpContext.Current.Request.Cookies["Language"];

            if (languageCookie == null) languageCookie = new HttpCookie("Language");

            languageCookie.Value = cultureName;

            languageCookie.Expires = DateTime.Now.AddDays(10);

            Response.SetCookie(languageCookie);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Language_OLD(string id)
        {
            EImeceLanguage selectedLanguage = (EImeceLanguage)id.ToInt();
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            CreateLanguageCookie(selectedLanguage, Constants.CultureCookieName);
            MemoryCacheProvider.ClearAll();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult OrderConfirmationEmail(int orderId = 1)
        {
            var emailTemplate = RazorEngineHelper.OrderConfirmationEmail(orderId);
            EmailSender.SendRenderedEmailTemplateToCustomer(SettingService.GetEmailAccount(), emailTemplate);
            return View(emailTemplate.Item2);
        }

        public ActionResult DisplayAllCache()
        {
            var cache = MemoryCache.Default;
            List<string> cacheKeys = cache.Select(kvp => kvp.Key).Where(r => r.Contains("Memory:")).ToList();
            List<string> keys = new List<string>();
            IDictionaryEnumerator enumerator = System.Web.HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string key = (string)enumerator.Key;
                keys.Add(key);
            }

            return View(new AllCacheList() { HttpRuntimeKey = keys, MemoryCacheKey = cacheKeys, ApproximateSize = GetApproximateSize(cache) });
        }
        
        public static long GetApproximateSize(MemoryCache cache)
        {
            try
            {
                var statsField = typeof(MemoryCache).GetField("_stats", BindingFlags.NonPublic | BindingFlags.Instance);
                var statsValue = statsField.GetValue(cache);
                var monitorField = statsValue.GetType().GetField("_cacheMemoryMonitor", BindingFlags.NonPublic | BindingFlags.Instance);
                var monitorValue = monitorField.GetValue(statsValue);
                var sizeField = monitorValue.GetType().GetField("_sizedRefMultiple", BindingFlags.NonPublic | BindingFlags.Instance);
                var sizeValue = sizeField.GetValue(monitorValue);
                var approxProp = sizeValue.GetType().GetProperty("ApproximateSize", BindingFlags.NonPublic | BindingFlags.Instance);
                return (long)approxProp.GetValue(sizeValue, null);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public class AllCacheList
        {
            public List<string> MemoryCacheKey;
            public List<string> HttpRuntimeKey;
            public long ApproximateSize;
        }
    }
}