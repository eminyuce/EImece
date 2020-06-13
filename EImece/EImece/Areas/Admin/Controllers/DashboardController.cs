using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{

    public class DashboardController : BaseAdminController
    {
        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.Title = "Gösterge Paneli";
            return View();
        }
        public ActionResult SearchContent(String searchContent)
        {
            String search = searchContent;
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

            Expression<Func<ProductCategory, bool>> whereLambda1 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(ProductCategoryService.SearchEntities(whereLambda1, search, CurrentLanguage));

            Expression<Func<Product, bool>> whereLambda2 = r => r.Name.Contains(search.Trim());
            resultList.AddRange(ProductService.SearchEntities(whereLambda2, search, CurrentLanguage));

            Expression<Func<StoryCategory, bool>> whereLambda3 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(StoryCategoryService.SearchEntities(whereLambda3, search, CurrentLanguage));

            Expression<Func<Story, bool>> whereLambda4 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(StoryService.SearchEntities(whereLambda4, search, CurrentLanguage));

            Expression<Func<Menu, bool>> whereLamba5 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(MenuService.SearchEntities(whereLamba5, search, CurrentLanguage));

            return View(resultList);
        }
        public ActionResult ClearCache()
        {
            MemoryCacheProvider.ClearAll();


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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            MemoryCacheProvider.ClearAll();
            return RedirectToAction("Index", "Home", new { @area = "" });
        }
        public ActionResult SetLanguage(string name)
        {
            //  name = CultureHelper.GetImplementedCulture(name);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(name);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            Response.Cookies[AdminCultureCookieName].Value = name;
            MemoryCacheProvider.ClearAll();
            var returnDefault = RedirectToAction("Index");
            return RequestReturn(returnDefault);
        }



    }
}