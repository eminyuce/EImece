using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;

namespace EImece.Controllers
{
    public class ProductCategoriesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // GET: ProductCategory
        public ActionResult Index()
        {
            return View();
        }
        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Category(String id)
        {
            var categoryId = id.GetId();

            ProductCategoryViewModel productCategory = ProductCategoryService.GetProductCategoryViewModel(categoryId);
            ViewBag.SeoId = productCategory.ProductCategory.GetSeoUrl();
            return View(productCategory);
        }
    }
}