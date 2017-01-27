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

        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Index(int page = 1)
        {
            var products = ProductService.GetMainPageProducts(page, CurrentLanguage);
            return View(products);
        }
        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Detail(String id)
        {
            var productId = id.GetId();
            var product = ProductService.GetProductById(productId);
            return View(product);
        }

        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Tag(String id)
        {
            var tagId = id.GetId();
            int pageIndex = 1;
            int pageSize = 20;
            SimiliarProductTagsViewModel products = ProductService.GetProductByTagId(tagId, pageIndex, pageSize, CurrentLanguage);
            return View(products);
        }
        public ActionResult SearchProducts(String search)
        {
            int pageIndex = 1;
            int pageSize = 20;
            ProductsSearchViewModel products = ProductService.SearchProducts(pageIndex, pageSize, search, CurrentLanguage);
            return View(products);
        }
    }
}