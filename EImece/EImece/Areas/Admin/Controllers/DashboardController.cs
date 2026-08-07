using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using EImece.Domain.DependencyInjection;
using NLog;
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

        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.Title = "Gösterge Paneli";
            return View();
        }

        [HttpGet]
        public ActionResult SearchContent(String searchContent)
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

                return RedirectToAction("Index");
            }
            List<BaseContent> resultList = SearchDatabaseForDashboard(search);

            return View(resultList);
        }

        private List<BaseContent> SearchDatabaseForDashboard(string search)
        {
            var resultList = new List<BaseContent>();
            Expression<Func<ProductCategory, bool>> whereLambda1 = r => r.Name.Contains(search);
            resultList.AddRange(ProductCategoryService.SearchEntities(whereLambda1, search, CurrentLanguage));

            Expression<Func<Product, bool>> whereLambda2 = r => r.Name.Contains(search) || r.NameLong.Contains(search);
            resultList.AddRange(ProductService.SearchEntities(whereLambda2, search, CurrentLanguage));

            Expression<Func<StoryCategory, bool>> whereLambda3 = r => r.Name.Contains(search);
            resultList.AddRange(StoryCategoryService.SearchEntities(whereLambda3, search, CurrentLanguage));

            Expression<Func<Story, bool>> whereLambda4 = r => r.Name.Contains(search);
            resultList.AddRange(StoryService.SearchEntities(whereLambda4, search, CurrentLanguage));

            Expression<Func<Menu, bool>> whereLamba5 = r => r.Name.Contains(search);
            resultList.AddRange(MenuService.SearchEntities(whereLamba5, search, CurrentLanguage));
            return resultList;
        }

        [HttpGet]
        public ActionResult ClearCache()
        {
            // Evict caches synchronously — this is fast and must complete before we redirect so the
            // admin immediately sees fresh data. The expensive rebuild is deferred to a background job.
            var evictionSw = System.Diagnostics.Stopwatch.StartNew();
            SettingService.ClearCache();
            MemoryCacheProvider.ClearAll();
            Logger.Info("ClearCache: cache eviction completed in {0} ms", evictionSw.ElapsedMilliseconds);

            // Capture request-bound values now; HttpContext is unavailable on the background thread.
            var baseUrl = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority);
            var language = CurrentLanguage;

            // Rebuild the cache off the request thread so the user gets an immediate response while
            // the (expensive) DB priming and sitemap crawl continue in the background.
            App_Start.CacheWarmUpJob.Queue(baseUrl, language);

            SetSuccessMessage();

            string redirectUrl;
            if (SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index");
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
            var returnDefault = RedirectToAction("Index");
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