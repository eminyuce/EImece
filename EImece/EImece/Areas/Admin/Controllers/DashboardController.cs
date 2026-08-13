using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string IndexAction = "Index";
        private const string AdminAreaName = "admin";

        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.Title = "Gösterge Paneli";
            ViewBag.IyzicoCredentialsMissing = string.Equals(AppConfig.PaymentProvider, "Iyzico", StringComparison.OrdinalIgnoreCase)
                && !AppConfig.HasConfiguredIyzicoCredentials;
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
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            MemoryCacheProvider.ClearAll();
            return RedirectToAction("Index", "Home", new { @area = "" });
        }

        public ActionResult OurSiteFeatures()
        {
            return View();
        }

        public PartialViewResult Languages()
        {
            List<SelectListItem> listItems = EnumHelper.ToSelectList3(Domain.Constants.AdminCultureCookieName);
            return PartialView("pLanguages", listItems);
        }

        [HttpGet]
        public ActionResult SetLanguage(string id)
        {
            EImeceLanguage selectedLanguage = (EImeceLanguage)id.ToInt();
            CreateLanguageCookie(selectedLanguage, Domain.Constants.AdminCultureCookieName);
            MemoryCacheProvider.ClearAll();
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
