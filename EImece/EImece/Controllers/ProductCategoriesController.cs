using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [RoutePrefix(Constants.ProductsCategoriesControllerRoutingPrefix)]
    public class ProductCategoriesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }

        [Inject]
        public IProductService ProductService { get; set; }

        public async Task<ActionResult> GetProductCategoryDto(String id)
        {
            var productCategory = await ProductCategoryService.GetProductCategoryDtoAsync(id.GetId());
            return View(productCategory);
        }

        /// <summary>
        /// Bare /c/pc/ (no slug) → home. Optional nice-to-have for bookmarks that omit the category id.
        /// </summary>
        [Route("pc")]
        public ActionResult CategoryRoot()
        {
            Logger.Info("CategoryRoot: redirecting bare /c/pc/ to home.");
            return RedirectToActionPermanent("Index", "Home");
        }

        /// <summary>
        /// Legacy bookmarks such as /c/Ev-Yasam → permanent redirect to /c/pc/{seo-hash}/ when resolvable.
        /// </summary>
        [Route("{slug}")]
        public async Task<ActionResult> CategoryLegacy(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug) || slug.Equals("pc", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToActionPermanent("Index", "Home");
                }

                var categoryId = slug.GetId();
                if (categoryId > 0)
                {
                    var byId = await ProductCategoryService.GetProductCategoryAsync(categoryId);
                    if (byId != null && byId.IsActive)
                    {
                        var canonicalFromId = Url.Action("Category", new { id = byId.GetSeoUrl() });
                        if (!string.IsNullOrEmpty(canonicalFromId))
                        {
                            return RedirectPermanent(canonicalFromId);
                        }
                    }
                }

                var tree = await ProductCategoryService.BuildTreeAsync(true, CurrentLanguage);
                var match = CategorySlugHelper.FindMatchingCategory(tree, slug);
                if (match == null)
                {
                    Logger.Info("CategoryLegacy: no category matched slug '{0}'.", slug);
                    return RedirectToAction("NotFound", "Error");
                }

                var destination = Url.Action("Category", new { id = match.GetSeoUrl() });
                if (string.IsNullOrEmpty(destination))
                {
                    return RedirectToAction("NotFound", "Error");
                }

                Logger.Info("CategoryLegacy: '{0}' → '{1}'.", slug, destination);
                return RedirectPermanent(destination);
            }
            catch (Exception ex)
            {
                return HandleUnexpectedError(ex, $"Exception in CategoryLegacy for slug: '{slug}'. Message: {ex.Message}");
            }
        }

        /// <summary>
        /// Permanent redirect from conventional /productcategories/category/{id} to /c/pc/{id}/.
        /// </summary>
        public ActionResult CategoryLegacyMvc(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToActionPermanent("Index", "Home");
            }

            var destination = Url.Action("Category", new { id });
            if (string.IsNullOrEmpty(destination))
            {
                return RedirectToAction("NotFound", "Error");
            }

            Logger.Info("CategoryLegacyMvc: '{0}' → '{1}'.", id, destination);
            return RedirectPermanent(destination);
        }

        [Route(Constants.CategoryPrefix)]
        public async Task<ActionResult> Category(String id, int page = 0, int sorting = 0, string filtreler = "", int minPrice = 0, int maxPrice = 0)
        {
            Logger.Info($"Entering Category action with id: '{id}', page: {page}, sorting: {sorting}, filtreler: '{filtreler}', minPrice: {minPrice}, maxPrice: {maxPrice}");
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    Logger.Info("Category ID is null or empty. Redirecting bare category URL to home.");
                    return RedirectToActionPermanent("Index", "Home");
                }

                var categoryId = id.GetId();
                Logger.Info($"Parsed category ID: {categoryId}");

                var productCategory = await ProductCategoryService.GetStorefrontCategoryPageViewModelAsync(
                    categoryId,
                    page > 0 ? page : 1,
                    (SortingType)sorting,
                    filtreler,
                    minPrice > 0 ? (int?)minPrice : null,
                    maxPrice > 0 ? (int?)maxPrice : null,
                    AppConfig.ProductDefaultRecordPerPage,
                    CurrentLanguage);

                Logger.Info($"Retrieved product category view model for ID: {categoryId}, Name: {productCategory?.CategoryDto?.Name}");

                if (productCategory == null || productCategory.CategoryDto == null || !productCategory.CategoryDto.IsActive)
                {
                    Logger.Info($"ProductCategory with ID: {categoryId} is null or inactive. Redirecting to NotFound error page.");
                    return RedirectToAction("NotFound", "Error");
                }

                productCategory.SeoId = id;
                ViewBag.SeoId = productCategory.CategoryDto.GetSeoUrl();

                SetCurrentCulture(productCategory.CategoryDto.Lang);
                Logger.Info("Set current culture based on product category.");

                Logger.Info("Returning Category view.");
                return View(productCategory);
            }
            catch (Exception ex)
            {
                return HandleUnexpectedError(ex, $"Exception in Category action for id: '{id}'. Message: {ex.Message}");
            }
        }
    }
}