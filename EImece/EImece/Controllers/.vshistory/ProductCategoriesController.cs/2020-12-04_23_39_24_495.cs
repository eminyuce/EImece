using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Net;
using System.Web.Mvc;
using System.Linq;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using Resources;
using EImece.Domain;
using EImece.Domain.Helpers.HtmlHelpers;
using EImece.Domain.Models;
using EImece.Domain.Models.Enums;
using System.Collections.Generic;

namespace EImece.Controllers
{
    [RoutePrefix(Constants.ProductsCategoriesControllerRoutingPrefix)]
    public class ProductCategoriesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }

        // GET: ProductCategory
        public ActionResult Index()
        {
            return View();
        }

        [Route(Constants.CategoryPrefix)]
        //[CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Category(String id, int page = 0, int sorting = 0, string filtreler = "", int minPrice = 0, int maxPrice = 0)
        {
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var categoryId = id.GetId();

                var productCategory = ProductCategoryService.GetProductCategoryViewModel(categoryId);
                productCategory.SeoId = id;
                productCategory.Page = page;
                productCategory.Filter = filtreler;
                productCategory.Sorting = (SortingType)sorting;
                if (minPrice > 0)
                {
                    productCategory.MinPrice = minPrice;
                }
                else
                {
                    productCategory.MinPrice = null;
                }
                if (maxPrice > 0)
                {
                    productCategory.MaxPrice = maxPrice;
                }
                else
                {
                    productCategory.MaxPrice = null;
                }
                productCategory.RecordPerPage = AppConfig.ProductDefaultRecordPerPage;

                ViewBag.SeoId = productCategory.ProductCategory.GetSeoUrl();

                List<Product> productsList = productCategory.ProductCategory.Products.ToList();
                productsList.AddRange(productCategory.CategoryChildrenProducts);
                productCategory.AllProducts = productsList;
                return View(productCategory);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " id:" + id);
                return RedirectToAction("InternalServerError", "Error");
            }
        }
    }
}