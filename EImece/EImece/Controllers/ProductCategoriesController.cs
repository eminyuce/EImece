using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

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
        public ActionResult GetProductCategoryDto(String id)
        {
            var productCategory = ProductCategoryService.GetProductCategoryDto(id.GetId());
            return View(productCategory);
        }

        [Route(Constants.CategoryPrefix)]
        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Category(String id, int page = 0, int sorting = 0, string filtreler = "", int minPrice = 0, int maxPrice = 0)
        {
            Logger.Info($"Entering Category action with id: '{id}', page: {page}, sorting: {sorting}, filtreler: '{filtreler}', minPrice: {minPrice}, maxPrice: {maxPrice}");
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    Logger.Error("Category ID is null or empty.");
                    Logger.Info("Returning BadRequest status.");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var categoryId = id.GetId();
                Logger.Info($"Parsed category ID: {categoryId}");

                // Use the DTO-based ViewModel
                var productCategory = ProductCategoryService.GetProductCategoryViewModelWithDtos(categoryId);
                Logger.Info($"Retrieved product category view model for ID: {categoryId}, Name: {productCategory?.ProductCategory?.Name}");

                productCategory.SeoId = id;
                productCategory.Page = page;
                productCategory.Filter = filtreler;
                productCategory.Sorting = (SortingType)sorting;
                Logger.Info($"Set initial properties: SeoId={id}, Page={page}, Filter='{filtreler}', Sorting={(SortingType)sorting}");

                if (minPrice > 0)
                {
                    productCategory.MinPrice = minPrice;
                    Logger.Info($"Set MinPrice: {minPrice}");
                }
                else
                {
                    productCategory.MinPrice = null;
                    Logger.Info("MinPrice set to null (minPrice <= 0).");
                }
                if (maxPrice > 0)
                {
                    productCategory.MaxPrice = maxPrice;
                    Logger.Info($"Set MaxPrice: {maxPrice}");
                }
                else
                {
                    productCategory.MaxPrice = null;
                    Logger.Info("MaxPrice set to null (maxPrice <= 0).");
                }
                productCategory.RecordPerPage = AppConfig.ProductDefaultRecordPerPage;
                Logger.Info($"Set RecordPerPage: {AppConfig.ProductDefaultRecordPerPage}");

                ViewBag.SeoId = productCategory.ProductCategory.SeoUrl; // Using DTO property
                Logger.Info($"Set ViewBag.SeoId: {ViewBag.SeoId}");

                // Using DTO collections
                List<ProductDto> productsList = productCategory.ProductIdsInCategory != null 
                    ? productCategory.AllProducts.Where(p => productCategory.ProductIdsInCategory.Contains(p.Id)).ToList()
                    : new List<ProductDto>();
                Logger.Info($"Retrieved {productsList.Count} products directly in category ID: {categoryId}");
                
                if (productCategory.CategoryChildrenProducts != null)
                {
                    productsList.AddRange(productCategory.CategoryChildrenProducts);
                    Logger.Info($"Added {productCategory.CategoryChildrenProducts.Count} child category products. Total products: {productsList.Count}");
                }
                
                productCategory.AllProducts = productsList;

                // Set culture based on DTO
                SetCurrentCultureFromDto(productCategory.ProductCategory);
                Logger.Info("Set current culture based on product category DTO.");

                Logger.Info("Returning Category view.");
                return View(productCategory);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Exception in Category action for id: '{id}'. Message: {ex.Message}");
                Logger.Info("Redirecting to InternalServerError error page.");
                return RedirectToAction("InternalServerError", "Error");
            }
        }
    }
}