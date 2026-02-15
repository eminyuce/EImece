using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
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

        [Inject]
        public MigrationRepository MigrationRepository { get; set; }

        [CustomOutputCache(CacheProfile = Constants.Cache1Hour)]
        public ActionResult Index()
        {
            MainPageViewModel mainPageModel = MainPageImageService.GetMainPageViewModelDto(CurrentLanguage);
            mainPageModel.CurrentLanguage = CurrentLanguage;
            ViewBag.Title = SettingService.GetSettingByKey(Constants.SiteIndexMetaTitle, CurrentLanguage).ToStr();
            ViewBag.Description = SettingService.GetSettingByKey(Constants.SiteIndexMetaDescription, CurrentLanguage).ToStr();
            ViewBag.Keywords = SettingService.GetSettingByKey(Constants.SiteIndexMetaKeywords, CurrentLanguage).ToStr();
            return View(mainPageModel);
        }

        [HttpPost]
        public ActionResult AddSubscriber(Subscriber subscriber)
        {
            var emailChecker = new EmailAddressAttribute();
            if (subscriber == null || string.IsNullOrEmpty(subscriber.Email.ToStr().Trim()) || !emailChecker.IsValid(subscriber.Email.ToStr().Trim()))
            {
                HomeLogger.Error($"Invalid subscriber data.BadRequest status. Subscriber: {subscriber?.Email ?? "null"}");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
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
                HomeLogger.Error("ID is null ThanksForSubscription.");
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
            var script = SettingService.GetSettingByKey(Constants.WhatsAppCommunicationScript).ToStr();
            return Content(script);
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
                    HomeLogger.Info("Current language is Turkish. Returning 'tr'.");
                    return Content("tr");

                case EImeceLanguage.English:
                    HomeLogger.Info("Current language is English. Returning 'en'.");
                    return Content("en");

                case EImeceLanguage.Russian:
                    HomeLogger.Info("Current language is Russian. Returning 'ru'.");
                    return Content("ru");

                case EImeceLanguage.German:
                    HomeLogger.Info("Current language is German. Returning 'de'.");
                    return Content("de");
            }
            HomeLogger.Info("Default case. Returning 'tr'.");
            return Content("tr");
        }

        [ChildActionOnly]
        public ActionResult Navigation(string lang)
        {
            var eImageLang = EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
            var menus = MenuService.BuildTree(true, eImageLang);
            var tree = ProductCategoryService.BuildNavigation(true, eImageLang);
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
        public ActionResult Footer(string lang)
        {
            var eImageLang = EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
            var footerViewModel = MainPageImageService.GetFooterViewModel(eImageLang);
            return PartialView("_Footer", footerViewModel);
        }

        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        public ActionResult GetCompanyName()
        {
            string companyName = SettingService.GetSettingByKey(Constants.CompanyName);
            HomeLogger.Info($"Retrieved company name: {companyName}");
            return Content(companyName);
        }

        public ActionResult WebSiteAddressInfo(bool isMobilePage = false)
        {
            var item = new SettingLayoutViewModel();
            item.isMobilePage = isMobilePage;
            item.WebSiteCompanyPhoneAndLocation = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyPhoneAndLocation);
            item.WebSiteCompanyEmailAddress = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyEmailAddress);
            HomeLogger.Info("Returning _WebSiteAddressInfo partial view.");
            return PartialView("_WebSiteAddressInfo", item);
        }

        [HttpPost]
        public ActionResult SendContactUs(ContactUsFormViewModel contact)
        {
            HomeLogger.Info("Entering SendContactUs POST action.");
            if (contact == null)
            {
                HomeLogger.Error("Contact form data is null.");
                HomeLogger.Info("Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string ipAddress = Request.Headers["X-Forwarded-For"];
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.UserHostAddress;
            }
            contact.IPAddress = ipAddress;
            if (Session[CaptchaContactUsLogin] == null || !Session[CaptchaContactUsLogin].ToString().Equals(contact.Captcha, StringComparison.InvariantCultureIgnoreCase))
            {
                HomeLogger.Error($"Captcha validation failed. Session: {Session[CaptchaContactUsLogin]}, Input: {contact.Captcha}");
                ModelState.AddModelError("Captcha", Resource.ContactUsWrongSumForSecurityQuestion);
                ModelState.AddModelError("", Resource.ContactUsWrongSumForSecurityQuestion);
                if (contact.ItemType == EImeceItemType.Product)
                {
                    HomeLogger.Info($"ItemType is Product with ID: {contact.ItemId}");
                    var product = ProductService.GetProductDetailViewModelById(contact.ItemId);
                    product.Contact = contact;
                    HomeLogger.Info("Returning Product Detail view with captcha error.");
                    return View("../Products/Detail", product);
                }
                else if (contact.ItemType == EImeceItemType.Menu)
                {
                    HomeLogger.Info($"ItemType is Menu with ID: {contact.ItemId}");
                    var page = MenuService.GetPageById(contact.ItemId);
                    page.Contact = contact;
                    HomeLogger.Info("Returning Page Detail view with captcha error.");
                    return View("../Pages/Detail", page);
                }
                HomeLogger.Info("Returning _ContactUsFormViewModel view with captcha error.");
                return View("_ContactUsFormViewModel", contact);
            }
            else if (!validateContactUsFormViewModel(contact))
            {
                HomeLogger.Info("Contact form validation failed.");
                HomeLogger.Info("Returning _ContactUsFormViewModel view with errors.");
                return View("_ContactUsFormViewModel", contact);
            }
            else
            {
                try
                {
                    HomeLogger.Info("Saving subscriber from contact form.");
                    saveSubsciber(contact);
                }
                catch (DbEntityValidationException ex)
                {
                    var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                    HomeLogger.Error($"DbEntityValidationException while saving subscriber: {message}", ex);
                }
                catch (Exception ex)
                {
                    HomeLogger.Error($"Exception while saving subscriber: {ex.Message}", ex);
                }

                try
                {
                    if (contact.ItemType == EImeceItemType.Product)
                    {
                        HomeLogger.Info($"Sending contact email for product ID: {contact.ItemId}");
                        RazorEngineHelper.SendContactUsAboutProductDetailEmail(contact);
                        HomeLogger.Info("Product contact email sent.");
                    }
                    else
                    {
                        HomeLogger.Info("Sending general contact email.");
                        RazorEngineHelper.SendContactUsForCommunication(contact);
                        HomeLogger.Info("General contact email sent.");
                    }
                }
                catch (Exception ex)
                {
                    HomeLogger.Error($"Exception while sending email: {ex.Message}", ex);
                }
                HomeLogger.Info("Returning _pThankYouForContactingUs view.");
                return View("_pThankYouForContactingUs", contact);
            }
        }

        private bool validateContactUsFormViewModel(ContactUsFormViewModel contact)
        {
            HomeLogger.Info("Entering validateContactUsFormViewModel method.");
            bool result = true;
            if (string.IsNullOrEmpty(contact.Email))
            {
                HomeLogger.Info("Email is empty. Adding error.");
                result = false;
                ModelState.AddModelError("Email", Resource.EmailRequired);
            }
            if (string.IsNullOrEmpty(contact.Name))
            {
                HomeLogger.Info("Name is empty. Adding error.");
                result = false;
                ModelState.AddModelError("Name", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(contact.Message))
            {
                HomeLogger.Info("Message is empty. Adding error.");
                result = false;
                ModelState.AddModelError("Message", Resource.ContactUsMessageErrorMessage);
            }
            HomeLogger.Info($"Validation result: {result}");
            return result;
        }

        private void saveSubsciber(ContactUsFormViewModel contact)
        {
            HomeLogger.Info("Entering saveSubsciber method.");
            var s = new Subscriber();
            s.Email = contact.Email.ToStr();
            s.CreatedDate = DateTime.Now;
            s.IsActive = true;
            s.Name = contact.Name.ToStr();
            s.UpdatedDate = DateTime.Now;
            s.Position = 1;
            s.Lang = CurrentLanguage;
            s.Note = string.Format("{0} {4} {1} {4} {2} {4} {3} ",
                contact.CompanyName, contact.Phone, contact.Address, contact.Message, Environment.NewLine);
            HomeLogger.Info($"Saving subscriber with email: {s.Email}");
            SubsciberService.SaveOrEditEntity(s);
            HomeLogger.Info("Subscriber saved successfully.");
        }

        public ActionResult Language(string id)
        {
            HomeLogger.Info($"Entering Language action with id: {id}");
            SetLanguage(id);
            MemoryCacheProvider.ClearAll();
            HomeLogger.Info("Language set and cache cleared.");
            HomeLogger.Info("Redirecting to Index.");
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Language_OLD(string id)
        {
            HomeLogger.Info($"Entering Language_OLD action with id: {id}");
            EImeceLanguage selectedLanguage = (EImeceLanguage)id.ToInt();
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            HomeLogger.Info($"Setting culture to: {cultureName}");
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            CreateLanguageCookie(selectedLanguage, Constants.CultureCookieName);
            MemoryCacheProvider.ClearAll();
            HomeLogger.Info("Language set, cookie created, and cache cleared.");
            HomeLogger.Info("Redirecting to Index.");
            return RedirectToAction("Index", "Home");
        }

        public ActionResult OrderConfirmationEmail(int orderId = 1)
        {
            HomeLogger.Info($"Entering OrderConfirmationEmail with orderId: {orderId}");
            var emailTemplate = RazorEngineHelper.OrderConfirmationEmail(orderId);
            HomeLogger.Info("Generated order confirmation email template.");
            EmailSender.SendRenderedEmailTemplateToCustomer(SettingService.GetEmailAccount(), emailTemplate);
            HomeLogger.Info("Order confirmation email sent to customer.");
            HomeLogger.Info("Returning email template view.");
            return View(emailTemplate.Item2);
        }

        public ActionResult DisplayAllCache()
        {
            HomeLogger.Info("Entering DisplayAllCache action.");
            var cache = MemoryCache.Default;
            List<string> cacheKeys = cache.Select(kvp => kvp.Key).Where(r => r.Contains("Memory:")).ToList();
            HomeLogger.Info($"Retrieved {cacheKeys.Count} MemoryCache keys.");
            List<string> keys = new List<string>();
            IDictionaryEnumerator enumerator = System.Web.HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string key = (string)enumerator.Key;
                keys.Add(key);
            }
            HomeLogger.Info($"Retrieved {keys.Count} HttpRuntime cache keys.");
            var approximateSize = GetApproximateSize(cache);
            HomeLogger.Info($"Calculated approximate cache size: {approximateSize}");
            HomeLogger.Info("Returning DisplayAllCache view.");
            return View(new AllCacheList() { HttpRuntimeKey = keys, MemoryCacheKey = cacheKeys, ApproximateSize = approximateSize });
        }

        public static long GetApproximateSize(MemoryCache cache)
        {
            HomeLogger.Info("Entering GetApproximateSize method.");
            try
            {
                var statsField = typeof(MemoryCache).GetField("_stats", BindingFlags.NonPublic | BindingFlags.Instance);
                var statsValue = statsField.GetValue(cache);
                var monitorField = statsValue.GetType().GetField("_cacheMemoryMonitor", BindingFlags.NonPublic | BindingFlags.Instance);
                var monitorValue = monitorField.GetValue(statsValue);
                var sizeField = monitorValue.GetType().GetField("_sizedRefMultiple", BindingFlags.NonPublic | BindingFlags.Instance);
                var sizeValue = sizeField.GetValue(monitorValue);
                var approxProp = sizeValue.GetType().GetProperty("ApproximateSize", BindingFlags.NonPublic | BindingFlags.Instance);
                long size = (long)approxProp.GetValue(sizeValue, null);
                HomeLogger.Info($"Calculated approximate size: {size}");
                return size;
            }
            catch (Exception ex)
            {
                HomeLogger.Error($"Exception in GetApproximateSize: {ex.Message}", ex);
                HomeLogger.Info("Returning -1 due to error.");
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