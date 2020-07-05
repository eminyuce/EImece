using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using System;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [RoutePrefix(Constants.ProductsControllerRoutingPrefix)]
    public class ProductsController : BaseController
    {
        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Index(int page = 1)
        {
            var products = ProductService.GetMainPageProducts(page, CurrentLanguage);
            return View(products);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult AdvancedSearchProducts(String search = "", string filters = "", String page = "")
        {
            var products = ProductService.GetProductsSearchResult(search, filters, page, CurrentLanguage);
            return View(products);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Detail(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var productId = id.GetId();
            var product = ProductService.GetProductById(productId);
            ViewBag.SeoId = product.Product.GetSeoUrl();

            return View(product);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Tag(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var tagId = id.GetId();
            int pageIndex = 1;
            int pageSize = 20;
            SimiliarProductTagsViewModel products = ProductService.GetProductByTagId(tagId, pageIndex, pageSize, CurrentLanguage);
            ViewBag.SeoId = products.Tag.GetSeoUrl();
            return View(products);
        }

        public ActionResult SearchProducts(String search)
        {
            if (String.IsNullOrEmpty(search))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int pageIndex = 1;
            int pageSize = 20;
            ProductsSearchViewModel products = ProductService.SearchProducts(pageIndex, pageSize, search, CurrentLanguage);
            return View(products);
        }
    }
}