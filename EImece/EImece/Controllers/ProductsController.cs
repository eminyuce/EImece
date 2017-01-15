using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class ProductsController : BaseController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // GET: Products
        public ActionResult Index(int page = 1)
        {
            var products = ProductService.GetMainPageProducts(page, Settings.RecordPerPage, Settings.MainLanguage);
            return View(products);
        }
        public ActionResult Category(String id)
        {
            var categoryId = id.Split("-".ToCharArray()).Last().ToInt();

            var productCategory = ProductCategoryService.GetProductCategory(categoryId);
            return View(productCategory);
        }
        public ActionResult Detail(String id)
        {
            var productId = id.Split("-".ToCharArray()).Last().ToInt();
            var product = ProductService.GetProductById(productId);
            return View(product);
        }
        public ActionResult Tag(String id)
        {
            var tagId = id.Split("-".ToCharArray()).Last().ToInt();
            int pageIndex = 1;
            int pageSize = 20;
            var lang = Settings.MainLanguage;
            SimiliarProductTagsViewModel products = ProductService.GetProductByTagId(tagId, pageIndex, pageSize, lang);
            return View(products);
        }
        public ActionResult SearchProducts(String search)
        {
            int pageIndex = 1;
            int pageSize = 20;
            var lang = Settings.MainLanguage;
            ProductsSearchViewModel products = ProductService.SearchProducts(pageIndex, pageSize, search, lang);
            return View(products);
        }
    }
}