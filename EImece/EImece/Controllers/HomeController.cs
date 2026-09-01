using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using EImece.Web.Controllers;
using EImece.Web.Filters;
using EImece.Web.Helpers;
using EImece.Web.Services;
using Microsoft.Extensions.Logging;
using Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IEimeceCacheProvider MemoryCacheProvider;
        private readonly IEmailSender EmailSender;
        private readonly ISubscriberService SubsciberService;
        private readonly IMainPageImageService MainPageImageService;
        private readonly IProductCategoryService ProductCategoryService;
        private readonly IMenuService MenuService;
        private readonly IMailTemplateService MailTemplateService;
        private readonly IRazorEngineHelper RazorEngineHelper;
        private readonly IProductService ProductService;

        public HomeController(ISettingService settingService,
            AutoMapper.IMapper mapper,
            IEimeceCacheProvider memoryCacheProvider,
            IEmailSender emailSender,
            ISubscriberService subsciberService,
            IMainPageImageService mainPageImageService,
            IProductCategoryService productCategoryService,
            IMenuService menuService,
            IMailTemplateService mailTemplateService,
            IRazorEngineHelper razorEngineHelper,
            IProductService productService, ILogger<HomeController> logger)
            : base(settingService, mapper, logger)
        {
            MemoryCacheProvider = memoryCacheProvider ?? throw new ArgumentNullException(nameof(memoryCacheProvider));
            EmailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            SubsciberService = subsciberService ?? throw new ArgumentNullException(nameof(subsciberService));
            MainPageImageService = mainPageImageService ?? throw new ArgumentNullException(nameof(mainPageImageService));
            ProductCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
            MenuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            MailTemplateService = mailTemplateService ?? throw new ArgumentNullException(nameof(mailTemplateService));
            RazorEngineHelper = razorEngineHelper ?? throw new ArgumentNullException(nameof(razorEngineHelper));
            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        [CustomOutputCache(CacheProfile = Constants.Cache1Hour)]
        public async Task<ActionResult> Index()
        {
            MainPageViewModel mainPageModel = await MainPageImageService.GetMainPageViewModelAsync(CurrentLanguage);
            mainPageModel.CurrentLanguage = CurrentLanguage;
            ViewBag.Title = (await SettingService.GetSettingByKeyAsync(Constants.SiteIndexMetaTitle, CurrentLanguage)).ToStr();
            ViewBag.Description = (await SettingService.GetSettingByKeyAsync(Constants.SiteIndexMetaDescription, CurrentLanguage)).ToStr();
            ViewBag.Keywords = (await SettingService.GetSettingByKeyAsync(Constants.SiteIndexMetaKeywords, CurrentLanguage)).ToStr();
            return View(mainPageModel);
        }

        [HttpPost]
        [RateLimit("contact", DefaultLimit = 3, DefaultWindowMinutes = 10)]
        public async Task<ActionResult> AddSubscriber(Subscriber subscriber)
        {
            var emailChecker = new EmailAddressAttribute();
            if (subscriber == null || string.IsNullOrEmpty(subscriber.Email.ToStr().Trim()) || !emailChecker.IsValid(subscriber.Email.ToStr().Trim()))
            {
                Logger.LogError($"Invalid subscriber data.BadRequest status. Subscriber: {subscriber?.Email ?? "null"}");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                subscriber.Name = subscriber.Email;
                subscriber.IsActive = true;
                await SubsciberService.SaveOrEditEntityAsync(subscriber);
                return RedirectToAction("ThanksForSubscription", new { id = subscriber.Id });
            }
        }

        public async Task<ActionResult> ThanksForSubscription(int? id)
        {
            if (!id.HasValue)
            {
                Logger.LogError("ID is null ThanksForSubscription.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var email = await SubsciberService.GetSubscriberEmailByIdAsync(id.Value);
            if (string.IsNullOrEmpty(email))
            {
                Logger.LogError($"Subscriber not found for ThanksForSubscription id={id.Value}.");
                return RedirectToAction("NotFound", "Error");
            }
            return View("ThanksForSubscription", email);
        }

        // Must stay synchronous: invoked via Html.Action child requests (MVC does not support async child actions).
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
            List<SelectListItem> listItems = EnumWebExtensions.ToSelectList3("Language");
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
        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "lang", VaryByCustom = "User")]
        public ActionResult Navigation(string lang)
        {
            var eImageLang = EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
            var menus = MenuService.BuildTree(true, eImageLang);
            var tree = ProductCategoryService.BuildNavigation(true, eImageLang);
            return PartialView("_Navigation", new NavigationModel(menus, tree));
        }

        [ChildActionOnly]
        // No [OutputCache] here: MVC forbids cached child actions nested inside another cached
        // child action (Crizal footer calls GetCompanyName). The data legs are service-cached.
        public ActionResult Footer(string lang)
        {
            var eImageLang = EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
            var footerViewModel = MainPageImageService.GetFooterViewModel(eImageLang);
            return PartialView("_Footer", footerViewModel);
        }

        // Must stay synchronous: invoked via Html.Action child requests (MVC does not support async child actions).
        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        public ActionResult GetCompanyName()
        {
            string companyName = SettingService.GetSettingByKey(Constants.CompanyName);
            return Content(companyName);
        }

        [ChildActionOnly]
        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "none", VaryByCustom = "User")]
        public ActionResult JsonLdOrganization()
        {
            var model = new JsonLdOrganizationModel
            {
                CompanyName = SettingService.GetCachedSettingValueDtoByKey(Constants.CompanyName).SettingValue,
                LogoSetting = SettingService.GetCachedSettingValueDtoByKey(Constants.WebSiteLogo).SettingValue,
                Phone = SettingService.GetCachedSettingValueDtoByKey(Constants.WebSiteCompanyPhoneAndLocation).SettingValue,
                Email = SettingService.GetCachedSettingValueDtoByKey(Constants.WebSiteCompanyEmailAddress).SettingValue
            };
            return PartialView("_JsonLdOrganization", model);
        }

        // Must stay synchronous: invoked via Html.Action child requests (MVC does not support async child actions).
        // Deliberately NOT [ChildActionOnly]: direct GETs render the partial harmlessly and the
        // regression inventory expects HTTP 200 on /home/websiteaddressinfo.
        [OutputCache(Duration = Constants.PartialViewOutputCachingDuration, VaryByParam = "isMobilePage", VaryByCustom = "User")]
        public ActionResult WebSiteAddressInfo(bool isMobilePage = false)
        {
            var item = new SettingLayoutViewModel();
            item.isMobilePage = isMobilePage;
            item.WebSiteCompanyPhoneAndLocation = SettingService.GetCachedSettingValueDtoByKey(Constants.WebSiteCompanyPhoneAndLocation);
            item.WebSiteCompanyEmailAddress = SettingService.GetCachedSettingValueDtoByKey(Constants.WebSiteCompanyEmailAddress);
            return PartialView("_WebSiteAddressInfo", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateCaptcha(Prefix = "ContactUsLogin")]
        [RateLimit("contact", DefaultLimit = 3, DefaultWindowMinutes = 10)]
        public async Task<ActionResult> SendContactUs(ContactUsFormViewModel contact)
        {
            if (contact == null)
            {
                Logger.LogError("Contact form data is null.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string ipAddress = Request.Headers["X-Forwarded-For"];
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.UserHostAddress;
            }
            contact.IPAddress = ipAddress;
            if (CaptchaService.HasValidationError(ModelState))
            {
                Logger.LogError("Captcha validation failed for SendContactUs.");
                ModelState.AddModelError("", CaptchaService.GetErrorMessage());
                return await ReturnContactCaptchaErrorAsync(contact);
            }
            else if (!validateContactUsFormViewModel(contact))
            {
                Logger.LogDebug("Contact form validation failed.");
                return View("_ContactUsFormViewModel", contact);
            }
            else
            {
                try
                {
                    await saveSubsciberAsync(contact);
                }
                catch (DbEntityValidationException ex)
                {
                    var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                    Logger.LogError($"DbEntityValidationException while saving subscriber: {message}", ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Exception while saving subscriber: {ex.Message}", ex);
                }

                try
                {
                    if (contact.ItemType == EImeceItemType.Product)
                    {
                        await RazorEngineHelper.SendContactUsAboutProductDetailEmailAsync(contact);
                        Logger.LogInformation("Product contact email sent. ProductId={0}", contact.ItemId);
                    }
                    else
                    {
                        await RazorEngineHelper.SendContactUsForCommunicationAsync(contact);
                        Logger.LogInformation("General contact email sent.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Exception while sending email: {ex.Message}", ex);
                }
                return View("_pThankYouForContactingUs", contact);
            }
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

        private async Task<ActionResult> ReturnContactCaptchaErrorAsync(ContactUsFormViewModel contact)
        {
            if (contact.ItemType == EImeceItemType.Product)
            {
                var product = await ProductService.GetProductDetailViewModelByIdAsync(contact.ItemId);
                product.Contact = contact;
                return View("../Products/Detail", product);
            }

            if (contact.ItemType == EImeceItemType.Menu)
            {
                var page = await MenuService.GetPageByIdAsync(contact.ItemId);
                if (page == null || page.Menu == null)
                {
                    Logger.LogWarning($"Menu page not found for contact ItemId: {contact.ItemId}");
                    return RedirectToAction("NotFound", "Error");
                }

                page.Contact = contact;
                return View("../Pages/Detail", page);
            }

            return View("_ContactUsFormViewModel", contact);
        }

        private async Task saveSubsciberAsync(ContactUsFormViewModel contact)
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
                contact.CompanyName, contact.Phone, contact.Address, contact.Message, Environment.NewLine);
            await SubsciberService.SaveOrEditEntityAsync(s);
            Logger.LogInformation("Subscriber saved successfully. Email={0}", s.Email);
        }

        public ActionResult Language(string id)
        {
            var contentLanguages = ContentLanguageSettingsHelper.GetCurrent();
            if (contentLanguages.IsBilingual)
            {
                var parsed = EnumHelper.ParseLanguage(id);
                if (parsed.HasValue && contentLanguages.IsLanguageEnabled(parsed.Value))
                {
                    SetLanguage(id);
                    MemoryCacheProvider.ClearAll();
                    Logger.LogInformation("Language set and cache cleared. Language={0}", id);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> OrderConfirmationEmail(int orderId = 1)
        {
            var emailTemplate = await RazorEngineHelper.OrderConfirmationEmailAsync(orderId);
            EmailSender.SendRenderedEmailTemplateToCustomer(await SettingService.GetEmailAccountAsync(), emailTemplate);
            Logger.LogInformation("Order confirmation email sent to customer. OrderId={0}", orderId);
            return View(emailTemplate.Item2);
        }

        public ActionResult DisplayAllCache()
        {
            var cacheKeys = CacheDiagnostics.GetMatchingEntries("", "all", "all")
                .Select(e => e.Key)
                .ToList();
            var keys = new List<string>();
            IDictionaryEnumerator enumerator = System.Web.HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string key = (string)enumerator.Key;
                keys.Add(key);
            }

            Logger.LogDebug("DisplayAllCache MemoryKeys={0} HttpRuntimeKeys={1}",
                cacheKeys.Count, keys.Count);
            return View(new AllCacheList() { HttpRuntimeKey = keys, MemoryCacheKey = cacheKeys, ApproximateSize = -1 });
        }

        public class AllCacheList
        {
            public List<string> MemoryCacheKey;
            public List<string> HttpRuntimeKey;
            public long ApproximateSize;
        }
    }
}