using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Entities;
using GenericRepository;

namespace EImece.Controllers
{
    public class ProductsController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Index(int page = 1)
        {
            try
            {
                var products = ProductService.GetMainPageProducts(page, CurrentLanguage);
                return View(products);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " page:" + page);
                return RedirectToAction("InternalServerError", "Error");
            }
        }
        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Detail(String id)
        {
            try
            {
                var productId = id.GetId();
                var product = ProductService.GetProductById(productId);
                ViewBag.SeoId = product.Product.GetSeoUrl();

                return View(product);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " id:" + id);
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Tag(String id)
        {
            try
            {
                var tagId = id.GetId();
                int pageIndex = 1;
                int pageSize = 20;
                SimiliarProductTagsViewModel products = ProductService.GetProductByTagId(tagId, pageIndex, pageSize, CurrentLanguage);
                ViewBag.SeoId = products.Tag.GetSeoUrl();
                return View(products);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " id:" + id);
                return RedirectToAction("InternalServerError", "Error");
            }
        }
        public ActionResult SearchProducts(String search)
        {
            try
            {
                int pageIndex = 1;
                int pageSize = 20;
                ProductsSearchViewModel products = ProductService.SearchProducts(pageIndex, pageSize, search, CurrentLanguage);
                return View(products);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " search:" + search);
                return RedirectToAction("InternalServerError", "Error");
            }
        }
    }
}