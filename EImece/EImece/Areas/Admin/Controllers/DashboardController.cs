using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using EImece.Domain.DependencyInjection;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string IndexAction = "Index";
        private const string AdminAreaName = "admin";

        protected IProductService ProductService { get; }
        protected IProductCategoryService ProductCategoryService { get; }
        protected IStoryService StoryService { get; }
        protected IStoryCategoryService StoryCategoryService { get; }
        protected IMenuService MenuService { get; }
        protected IEimeceCacheProvider MemoryCacheProvider { get; }

        public DashboardController(
            ISettingService settingService,
            IProductService productService,
            IProductCategoryService productCategoryService,
            IStoryService storyService,
            IStoryCategoryService storyCategoryService,
            IMenuService menuService,
            IEimeceCacheProvider memoryCacheProvider)
            : base(settingService)
        {
            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
            ProductCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
            StoryService = storyService ?? throw new ArgumentNullException(nameof(storyService));
            StoryCategoryService = storyCategoryService ?? throw new ArgumentNullException(nameof(storyCategoryService));
            MenuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            MemoryCacheProvider = memoryCacheProvider ?? throw new ArgumentNullException(nameof(memoryCacheProvider));
        }

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.Title = Resources.AdminResource.Dashboard;
            var paymentProvider = SettingService?.GetSettingByKey(Domain.Constants.PaymentProvider) ?? Domain.Constants.DefaultPaymentProvider;
            ViewBag.IyzicoCredentialsMissing = string.Equals(paymentProvider, "Iyzico", StringComparison.OrdinalIgnoreCase)
                && !AppConfig.HasConfiguredIyzicoCredentials;
            return View();
        }

        // GET: Admin/Dashboard/SystemHealth
        [HttpGet]
        public async Task<ActionResult> SystemHealth()
        {
            ViewBag.Title = Resources.AdminResource.SystemHealth;
            var underConstructionValue = await SettingService.GetSettingByKeyAsync(Domain.Constants.IsSiteUnderConstruction).ConfigureAwait(false);
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
        /// Admin top-bar Refresh button. Wipes every in-process cache layer (data + OutputCache)
        /// then shows a short refresh animation while background warm-up starts, then returns
        /// the admin to a safe page (never a POST-only URL such as UploadWebSiteLogo).
        /// </summary>
        [HttpGet]
        public ActionResult ClearCache()
        {
            // Evict caches synchronously — this is fast and must complete before we redirect so the
            // admin immediately sees fresh data. The expensive rebuild is deferred to a background job.
            var evictionSw = System.Diagnostics.Stopwatch.StartNew();

            // Targeted setting keys first (explicit), then the full provider wipe which also clears
            // ASP.NET OutputCache / HttpRuntime.Cache and MemoryCache.Default. Without OutputCache
            // eviction, [CustomOutputCache] product/home pages would keep serving stale HTML.
            SettingService.ClearCache();
            ProductService.InvalidateProductListCaches();
            var dataKeysRemoved = MemoryCacheProvider.ClearAll();
            Logger.Info(
                "ClearCache: eviction completed in {0} ms (provider data keys removed: {1})",
                evictionSw.ElapsedMilliseconds,
                dataKeysRemoved);

            // Capture request-bound values now; HttpContext is unavailable on the background thread.
            var baseUrl = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority);
            var language = CurrentLanguage;

            // Rebuild the cache off the request thread so the user gets an immediate response while
            // the (expensive) DB priming and sitemap crawl continue in the background.
            App_Start.CacheWarmUpJob.Queue(baseUrl, language);

            string redirectUrl;
            if (!SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                redirectUrl = Url.Action(IndexAction, "Dashboard", new { area = AdminAreaName });
            }

            redirectUrl = NormalizeClearCacheReturnUrl(redirectUrl);

            ViewBag.Title = AdminResource.Refresh;
            ViewBag.ReturnUrl = redirectUrl;
            return View();
        }

        /// <summary>
        /// Targeted storefront cache invalidation. Unlike <see cref="ClearCache"/> (full wipe +
        /// background warm-up), this evicts only the cache family the admin chooses, plus the
        /// rendered OutputCache HTML that embeds it, so anonymous visitors see fresh pages on
        /// their next request without a full warm-up crawl. Data caches stay warm for untouched
        /// families.
        /// Authorized via BaseAdminController ([AuthorizeRoles(Administrator, Editor)]) and
        /// POST + antiforgery so the operation cannot be triggered by link/GET requests.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InvalidateCache(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var removed = 0;
            bool fullWipe = false;

            switch (target.Trim().ToLowerInvariant())
            {
                case "products":
                    // Includes pricing: price/discount values live inside the product DTO caches,
                    // and UpdatePrices already funnels through InvalidateProductListCaches.
                    ProductService.InvalidateProductListCaches();
                    break;

                case "categories":
                    ProductCategoryService.InvalidateCategoryCaches();
                    break;

                case "settings":
                    SettingService.ClearCache();
                    break;

                case "content":
                    // Stories, menus/pages, banners, FAQ, tags and brands: everything rendered
                    // around the catalog but not part of product/category data.
                    removed += MemoryCacheProvider.ClearByPrefix(CacheKeys.StoryPrefix);
                    removed += MemoryCacheProvider.ClearByPrefix(CacheKeys.MenuPrefix);
                    removed += MemoryCacheProvider.ClearByPrefix(CacheKeys.BannerPrefix);
                    removed += MemoryCacheProvider.ClearByPrefix(CacheKeys.FaqPrefix);
                    removed += MemoryCacheProvider.ClearByPrefix(CacheKeys.TagPrefix);
                    removed += MemoryCacheProvider.ClearByPrefix(CacheKeys.BrandPrefix);
                    break;

                case "all":
                    // Full storefront invalidation: same flow as the top-bar Refresh button —
                    // every provider entry + OutputCache/MemoryCache.Default + background warm-up.
                    SettingService.ClearCache();
                    ProductService.InvalidateProductListCaches();
                    removed = MemoryCacheProvider.ClearAll();
                    fullWipe = true;
                    break;

                default:
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Targeted purges must also drop rendered HTML: product/category/story/menu pages are
            // OutputCached for 20 minutes for anonymous users, so without HttpRuntime eviction a
            // data-only purge would leave stale storefront pages visible until profile expiry.
            if (!fullWipe)
            {
                int htmlRemoved;
                int memoryDefaultRemoved;
                ApplicationCacheClearer.ClearAspNetCaches(out htmlRemoved, out memoryDefaultRemoved);
            }

            Logger.Info(
                "InvalidateCache target={0} removed={1} in {2} ms (fullWipe={3}) by {4}",
                target,
                removed,
                sw.ElapsedMilliseconds,
                fullWipe,
                User?.Identity?.Name ?? "unknown");

            if (fullWipe)
            {
                var baseUrl = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority);
                App_Start.CacheWarmUpJob.Queue(baseUrl, CurrentLanguage);
            }

            string redirectUrl;
            if (!SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                redirectUrl = Url.Action(IndexAction, "Dashboard", new { area = AdminAreaName });
            }

            SetSuccessMessage(string.Format(
                "Önbellek temizlendi ({0}). Storefront bir sonraki istekte güncel veriyi yükleyecek.",
                System.Web.HttpUtility.HtmlEncode(target)));

            return Redirect(redirectUrl);
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
                Logger.Warn(ex, "NormalizeClearCacheReturnUrl failed for {0}", redirectUrl);
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
