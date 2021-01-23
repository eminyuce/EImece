using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
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
            var resultList = new List<BaseContent>();
            ViewBag.SearchKey = search;
            if (String.IsNullOrEmpty(search))
            {
                var urlReferrer = Request.UrlReferrer;
                if (urlReferrer != null)
                {
                    return Redirect(urlReferrer.ToStr());
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

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

            return View(resultList);
        }

        [HttpGet]
        public ActionResult ClearCache()
        {
            MemoryCacheProvider.ClearAll();

            var urlReferrer = Request.UrlReferrer;
            ExecuteWarmUpSql();

            if (urlReferrer != null)
            {
                return Redirect(urlReferrer.ToStr());
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        private void ExecuteWarmUpSql()
        {
            try
            {
                SettingService.GetAllActiveSettings();
                MainPageImageService.GetMainPageViewModel(CurrentLanguage);
                var activeCategories = ProductCategoryService.GetActiveBaseContents(true, CurrentLanguage);
                if (activeCategories.IsNotEmpty())
                {
                    foreach (var c in activeCategories)
                    {
                        ProductCategoryService.GetProductCategoryViewModelWithCache(c.Id);
                    }
                }
                MenuService.GetActiveBaseContentsFromCache(true, CurrentLanguage);
                var products = ProductService.GetActiveProducts(CurrentLanguage);
                MailTemplateService.GetActiveBaseEntitiesFromCache
                if (products.IsNotEmpty())
                {
                    int i = 0;
                    foreach (var p in products)
                    {
                        ProductService.GetProductDetailViewModelById(p.Id);
                        if (i == 3)
                        {
                            break;
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ExecuteWarmUpSql error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            MemoryCacheProvider.ClearAll();
            return RedirectToAction("Index", "Home", new { @area = "" });
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