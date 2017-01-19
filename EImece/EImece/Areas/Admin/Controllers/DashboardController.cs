using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{

    public class DashboardController : BaseAdminController
    {
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
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
            resultList.AddRange(ProductCategoryService.SearchEntities(whereLambda1, search));

            Expression<Func<Product, bool>> whereLambda2 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(ProductService.SearchEntities(whereLambda2, search));

            Expression<Func<StoryCategory, bool>> whereLambda3 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(StoryCategoryService.SearchEntities(whereLambda3, search));

            Expression<Func<Story, bool>> whereLambda4 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(StoryService.SearchEntities(whereLambda4, search));

            Expression<Func<Menu, bool>> whereLamba5 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(MenuService.SearchEntities(whereLamba5, search));

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
    }
}