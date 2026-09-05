using EImece.Domain;
using EImece.Domain.Abstractions;
using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using EImece.Web.Areas.Admin.Controllers;
using EImece.Web.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        private const string IndexAction = "Index";
        private const string AdminAreaName = "admin";

        protected IProductService ProductService { get; }
        protected IProductCategoryService ProductCategoryService { get; }
        protected IStoryService StoryService { get; }
        protected IStoryCategoryService StoryCategoryService { get; }
        protected IMenuService MenuService { get; }
        protected IEimeceCacheProvider MemoryCacheProvider { get; }
        private readonly IHttpRuntimeCacheClearer _httpRuntimeCacheClearer;

        public DashboardController(ISettingService settingService,
            IProductService productService,
            IProductCategoryService productCategoryService,
            IStoryService storyService,
            IStoryCategoryService storyCategoryService,
            IMenuService menuService,
            IEimeceCacheProvider memoryCacheProvider,
            IHttpRuntimeCacheClearer httpRuntimeCacheClearer, ILogger<DashboardController> logger)
            : base(settingService, logger)
        {
            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
            ProductCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
            StoryService = storyService ?? throw new ArgumentNullException(nameof(storyService));
            StoryCategoryService = storyCategoryService ?? throw new ArgumentNullException(nameof(storyCategoryService));
            MenuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            MemoryCacheProvider = memoryCacheProvider ?? throw new ArgumentNullException(nameof(memoryCacheProvider));
            _httpRuntimeCacheClearer = httpRuntimeCacheClearer;
        }

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.Title = Resources.AdminResource.Dashboard;
            var paymentProvider = SettingService?.GetSettingByKeyFromDb(Domain.Constants.PaymentProvider) ?? Domain.Constants.DefaultPaymentProvider;
            ViewBag.IyzicoCredentialsMissing = string.Equals(paymentProvider, "Iyzico", StringComparison.OrdinalIgnoreCase)
                && !AppConfig.HasConfiguredIyzicoCredentials;
            return View();
        }

        // GET: Admin/Dashboard/SystemHealth
        [HttpGet]
        public async Task<ActionResult> SystemHealth()
        {
            ViewBag.Title = Resources.AdminResource.SystemHealth;
            var underConstructionValue = await SettingService.GetSettingByKeyFromDbAsync(Domain.Constants.IsSiteUnderConstruction).ConfigureAwait(false);
            ViewBag.IsSiteUnderConstruction = underConstructionValue.ToBool(false);
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> SearchContent(CancellationToken cancellationToken, String searchContent)
        {
            String search = searchContent.ToStr().Trim();

            ViewBag.SearchKey = search;
            if (String.IsNullOrEmpty(search))
            {
                string redirectUrl;
                if (SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
                {
                    return Redirect(redirectUrl);
                }

                return RedirectToAction(IndexAction);
            }
            List<BaseContent> resultList = await SearchDatabaseForDashboardAsync(search);

            return View(resultList);
        }

        private async Task<List<BaseContent>> SearchDatabaseForDashboardAsync(string search)
        {
            var resultList = new List<BaseContent>();
            Expression<Func<ProductCategory, bool>> whereLambda1 = r => r.Name.Contains(search);
            resultList.AddRange(await ProductCategoryService.SearchEntitiesAsync(whereLambda1, search, CurrentLanguage));

            Expression<Func<Product, bool>> whereLambda2 = r => r.Name.Contains(search) || r.NameLong.Contains(search);
            resultList.AddRange(await ProductService.SearchEntitiesAsync(whereLambda2, search, CurrentLanguage));

            Expression<Func<StoryCategory, bool>> whereLambda3 = r => r.Name.Contains(search);
            resultList.AddRange(await StoryCategoryService.SearchEntitiesAsync(whereLambda3, search, CurrentLanguage));

            Expression<Func<Story, bool>> whereLambda4 = r => r.Name.Contains(search);
            resultList.AddRange(await StoryService.SearchEntitiesAsync(whereLambda4, search, CurrentLanguage));

            Expression<Func<Menu, bool>> whereLamba5 = r => r.Name.Contains(search);
            resultList.AddRange(await MenuService.SearchEntitiesAsync(whereLamba5, search, CurrentLanguage));
            return resultList;
        }

        /// <summary>
        /// Legacy alias for existing bookmarks. Eviction uses the same
        /// <see cref="AdminCacheMaintenance"/> path as Cache Admin.
        /// </summary>
        [HttpGet]
        public ActionResult ClearCache()
        {
            var evictionSw = System.Diagnostics.Stopwatch.StartNew();
            var dataKeysRemoved = AdminCacheMaintenance.ClearAllData(SettingService, ProductService, MemoryCacheProvider);
            Logger.LogInformation(
                "ClearCache: eviction completed in {0} ms (provider data keys removed: {1})",
                evictionSw.ElapsedMilliseconds,
                dataKeysRemoved);

            var baseUrl = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority);
            App_Start.CacheWarmUpJob.Queue(baseUrl, CurrentLanguage);

            string redirectUrl;
            if (!SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                redirectUrl = Url.Action("Index", "Cache", new { area = AdminAreaName });
            }

            redirectUrl = NormalizeClearCacheReturnUrl(redirectUrl);

            ViewBag.Title = AdminResource.Refresh;
            ViewBag.ReturnUrl = redirectUrl;
            return View();
        }

        /// <summary>
        /// Map POST-only form URLs (e.g. /admin/settings/uploadwebsitelogo/) back to a GET page.
        /// </summary>
        private string NormalizeClearCacheReturnUrl(string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                return Url.Action(IndexAction, "Dashboard", new { area = AdminAreaName });
            }

            try
            {
                var uri = new Uri(redirectUrl, UriKind.RelativeOrAbsolute);
                var path = uri.IsAbsoluteUri ? uri.AbsolutePath : redirectUrl.Split('?')[0];
                if (path.IndexOf("uploadwebsitelogo", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var id = 0;
                    if (uri.IsAbsoluteUri && !string.IsNullOrEmpty(uri.Query))
                    {
                        var query = HttpUtility.ParseQueryString(uri.Query);
                        int.TryParse(query["id"], out id);
                    }

                    if (id > 0)
                    {
                        return Url.Action("WebSiteLogo", "Settings", new { area = AdminAreaName, id });
                    }

                    return Url.Action("AddWebSiteLogo", "Settings", new { area = AdminAreaName });
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "NormalizeClearCacheReturnUrl failed for {0}", redirectUrl);
                return Url.Action(IndexAction, "Dashboard", new { area = AdminAreaName });
            }

            return redirectUrl;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            MemoryCacheProvider.ClearAll();
            return RedirectToAction("Index", "Home", new { @area = "" });
        }

        public ActionResult OurSiteFeatures()
        {
            return View();
        }

        public PartialViewResult Languages()
        {
            List<SelectListItem> listItems = EnumWebExtensions.ToSelectList3(Domain.Constants.AdminCultureCookieName);
            return PartialView("pLanguages", listItems);
        }

        [HttpGet]
        public ActionResult SetLanguage(string id)
        {
            var contentLanguages = ContentLanguageSettingsHelper.GetCurrent();
            if (contentLanguages.IsBilingual)
            {
                EImeceLanguage selectedLanguage = (EImeceLanguage)id.ToInt();
                if (contentLanguages.IsLanguageEnabled(selectedLanguage))
                {
                    CreateLanguageCookie(selectedLanguage, Domain.Constants.AdminCultureCookieName);
                    MemoryCacheProvider.ClearAll();
                }
            }
            var returnDefault = RedirectToAction(IndexAction);
            return RequestReturn(returnDefault);
        }

        public void CreateLanguageCookie(EImeceLanguage selectedLanguage, string cookieName)
        {
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            var cultureCookie = new HttpCookie(cookieName);
            cultureCookie.Values[Domain.Constants.ELanguage] = ((int)selectedLanguage) + "";
            cultureCookie.Values["LastVisit"] = DateTime.Now.ToString();
            cultureCookie.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(cultureCookie);
        }
    }
}
