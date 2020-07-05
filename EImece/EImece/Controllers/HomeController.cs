using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class HomeController : BaseController
    {
        private const string CaptchaContactUsLogin = "CaptchaContactUsLogin";
        [Inject]
        public ICacheProvider MemoryCacheProvider { get; set; }
        private static readonly Logger HomeLogger = LogManager.GetCurrentClassLogger();

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Index()
        {
             
            MainPageViewModel mainPageModel = MainPageImageService.GetMainPageViewModel(CurrentLanguage);
            mainPageModel.CurrentLanguage = CurrentLanguage;
            ViewBag.Title = SettingService.GetSettingByKey(Constants.SiteIndexMetaTitle).ToStr();
            ViewBag.Description = SettingService.GetSettingByKey(Constants.SiteIndexMetaDescription).ToStr();
            ViewBag.Keywords = SettingService.GetSettingByKey(Constants.SiteIndexMetaKeywords).ToStr();

            return View(mainPageModel);
        }

        public async Task<ActionResult> About(String id = "2,4")
        {
            ViewBag.Message = "Your app description page.";

            var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });
            var request = new HttpRequestMessage { RequestUri = new Uri("http://dev2.marinelink.com/api/podcastapi/get") };

            var m = id.Split(",".ToCharArray());
            request.Headers.Range = new RangeHeaderValue(m[0].ToInt(), m[1].ToInt());

            var ee = request.Headers.Range.Ranges;
            var response = await client.SendAsync(request);
            var h = response.Headers;
            var cl = response.Content.Headers.ContentLength;
            var r = response.RequestMessage;

            HttpContent requestContent = response.Content;
            string jsonContent = requestContent.ReadAsStringAsync().Result;

            return Json(jsonContent, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult AddSubscriber(Subscriber subscriber)
        {
            //validate the captcha through the session variable stored from GetCaptcha
            subscriber.Name = subscriber.Email;
            subscriber.IsActive = true;
            SubsciberService.SaveOrEditEntity(subscriber);
            return RedirectToAction("ThanksForSubscription", new { id = subscriber.Id });
        }

        public ActionResult ThanksForSubscription(int ? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var s = SubsciberService.GetSingle(id.Value);
            return View(s);
        }

        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        public ActionResult SocialMediaLinks()
        {
            var InstagramWebSiteLink = SettingService.GetSettingByKey(Constants.InstagramWebSiteLink);
            var TwitterWebSiteLink = SettingService.GetSettingByKey(Constants.TwitterWebSiteLink);
            var LinkedinWebSiteLink = SettingService.GetSettingByKey(Constants.LinkedinWebSiteLink);
            var FacebookWebSiteLink = SettingService.GetSettingByKey(Constants.FacebookWebSiteLink);

            var resultList = new List<String>();
            resultList.Add(InstagramWebSiteLink);
            resultList.Add(TwitterWebSiteLink);
            resultList.Add(LinkedinWebSiteLink);
            resultList.Add(FacebookWebSiteLink);

            return PartialView("_SocialMediaLinks", resultList);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public ActionResult AboutUs()
        {
            var setting = SettingService.GetSettingObjectByKey(Constants.AboutUs, CurrentLanguage);
            return View(setting);
        }
        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public ActionResult TermsAndConditions()
        {
            var setting = SettingService.GetSettingObjectByKey(Constants.TermsAndConditions, CurrentLanguage);
            return View(setting);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public ActionResult PrivacyPolicy()
        {
            var setting = SettingService.GetSettingObjectByKey(Constants.PrivacyPolicy, CurrentLanguage);
            return View(setting);
        }

        [ChildActionOnly]
        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        public ActionResult GoogleAnalyticsTrackingScript()
        {
            var GoogleAnalyticsTrackingScript = SettingService.GetSettingByKey(Constants.GoogleAnalyticsTrackingScript).ToStr();
            return Content(GoogleAnalyticsTrackingScript);
        }
        public ActionResult Languages()
        {
            List<SelectListItem> listItems = EnumHelper.ToSelectList3(Constants.CultureCookieName);
            return PartialView("_Languages", listItems);
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

        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        public ActionResult WebSiteLogo()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
            var CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName);
            var s = new List<Setting>() { webSiteLogo, CompanyName };
            return PartialView("_WebSiteLogo", s);
        }

        public ActionResult Footer()
        {
            FooterViewModel footerViewModel = MainPageImageService.GetFooterViewModel(CurrentLanguage);
            return PartialView("_Footer", footerViewModel);
        }

        public ActionResult WebSiteAddressInfo()
        {
            var WebSiteCompanyPhoneAndLocation = SettingService.GetSettingByKey(Constants.WebSiteCompanyPhoneAndLocation);
            var WebSiteCompanyEmailAddress = SettingService.GetSettingByKey(Constants.WebSiteCompanyEmailAddress);
            var resultList = new List<String>();
            resultList.Add(WebSiteCompanyPhoneAndLocation);
            resultList.Add(WebSiteCompanyEmailAddress);
            return PartialView("_WebSiteAddressInfo", resultList);
        }

        //[OutputCache(Duration = Settings.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        //public ActionResult ContactUs()
        //{
        //    ContactUsFormViewModel contact = new ContactUsFormViewModel();
        //    return PartialView("_ContactUsFormViewModel", contact);
        //}
        public ActionResult SendContactUs(ContactUsFormViewModel contact)
        {
            if (contact == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Session[CaptchaContactUsLogin] == null || !Session[CaptchaContactUsLogin].ToString().Equals(contact.Captcha, StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("Captcha", Resources.Resource.ContactUsWrongSumForSecurityQuestion);
                if (contact.ItemType == EImeceItemType.Product)
                {
                    var product = ProductService.GetProductById(contact.ItemId);
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
                        EmailSender.SendEmailContactingUs(contact);
                    }
                }
                catch (Exception ex)
                {
                    HomeLogger.Error(ex, "Exception Message:" + ex.Message);
                }
            }

            return View("_pThankYouForContactingUs", contact);
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
            s.EntityHash = "";
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
            String cultureName=EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            CreateLanguageCookie(selectedLanguage, Constants.CultureCookieName);
            MemoryCacheProvider.ClearAll();
       
            return RedirectToAction("Index","Home");
        }
    }
}