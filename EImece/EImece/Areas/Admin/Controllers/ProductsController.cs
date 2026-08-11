using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ProductsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public async Task<ActionResult> Index(
            CancellationToken cancellationToken,
            int id = 0,
            int brandId = -1,
            String search = "",
            string state = "",
            bool? isActive = null,
            bool? mainPage = null,
            bool? isCampaign = null,
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            var isProductPriceEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductPriceEnable);
            bool priceEnabled = isProductPriceEnable == null || isProductPriceEnable.SettingValue.ToBool(true);

            var filter = new ProductAdminListFilter
            {
                State = state,
                IsActive = isActive == true ? true : (bool?)null,
                MainPage = mainPage == true ? true : (bool?)null,
                IsCampaign = isCampaign == true ? true : (bool?)null,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                ApplyPriceFilter = priceEnabled
            };

            ViewBag.ProductCategoryTree = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            var products = await ProductService.GetAdminPageListAsync(id, brandId, search, CurrentLanguage, filter, cancellationToken);
            ViewBag.IsProductPriceEnable = isProductPriceEnable;
            ViewBag.SelectedCategory = await ProductCategoryService.GetSingleAsync(id);
            ViewBag.SelectedBrandId = brandId;
            ViewBag.SelectedState = state.ToStr();
            ViewBag.FilterIsActive = filter.IsActive;
            ViewBag.FilterMainPage = filter.MainPage;
            ViewBag.FilterIsCampaign = filter.IsCampaign;
            ViewBag.MinPrice = priceEnabled ? minPrice : null;
            ViewBag.MaxPrice = priceEnabled ? maxPrice : null;
            ViewBag.ProductFilter = filter;
            ViewBag.Brands = (await BrandService.GetBrandsIfAnyProductExistsAsync(CurrentLanguage, id))
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToList();
            ViewBag.ProductStates = Enum.GetValues(typeof(ProductState))
                .Cast<ProductState>()
                .ToList();
            return View(products);
        }

        [HttpGet]
        public async Task<ActionResult> SaveOrEditProductSpecs(CancellationToken cancellationToken, int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var productDetailViewModel = await ProductService.GetProductDetailViewModelByIdAsync(id, cancellationToken);
            Product content = productDetailViewModel.Product;
            ViewBag.Template = await TemplateService.GetTemplateAsync(content.ProductCategory.TemplateId.Value, cancellationToken);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEditProductSpecs(CancellationToken cancellationToken, int id, int templateId, String saveButton = null)
        {
            int productId = id;
            await ProductService.ParseTemplateAndSaveProductSpecificationsAsync(productId, templateId, CurrentLanguage, Request, cancellationToken);

            if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            var productDetailViewModel = await ProductService.GetProductDetailViewModelByIdAsync(id, cancellationToken);
            Product content = productDetailViewModel.Product;
            ViewBag.Template = await TemplateService.GetTemplateAsync(content.ProductCategory.TemplateId.Value, cancellationToken);
            RemoveModelState();
            return View(content);
        }

        //
        // GET: /Product/Create
        [HttpGet]
        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<Product>();
            ViewBag.Brands = await GetBrandsSelectListAsync();
            var productCategory = EntityFactory.GetBaseContentInstance<ProductCategory>();
            ViewBag.ProductCategoryTree = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            if (id == 0)
            {
                content.ProductCategoryId = 0;
            }
            else
            {
                content = await ProductService.GetBaseContentAsync(id, cancellationToken);
                content.PriceStr = decimal.Round(content.Price, 2, MidpointRounding.AwayFromZero).ToString().Replace(".", ",");
                content.DiscountStr = content.Discount.HasValue ? decimal.Round(content.Discount.Value, 2, MidpointRounding.AwayFromZero).ToString().Replace(".", ",") : "";
                productCategory = await ProductCategoryService.GetSingleAsync(content.ProductCategoryId);
            }
            ViewBag.ProductCategory = productCategory;
            ViewBag.IsProductPriceEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductPriceEnable);
            return View(content);
        }

        //
        // POST: /Product/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, Product product, int[] tags = null, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            int contentId = 0;
            if (product == null)
            {
                return HttpNotFound();
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var isProductPriceEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductPriceEnable);
                    if (product.ProductCategoryId == 0)
                    {
                        ModelState.AddModelError("ProductCategoryId", AdminResource.ProductCategoryIdErrorMessage);
                        ModelState.AddModelError("", AdminResource.ProductCategoryIdErrorMessage);
                    }
                    //     else if (isProductPriceEnable.SettingValue.ToBool(false) && product.Price <= 0)
                    //   {
                    //        ModelState.AddModelError("Price", AdminResource.PriceErrorMessage);
                    //         ModelState.AddModelError("", AdminResource.PriceErrorMessage);
                    //      }
                    else
                    {
                        FilesHelper.SaveFileFromHttpPostedFileBase(
                             postedImage,
                             product.ImageHeight,
                             product.ImageWidth,
                             EImeceImageType.ProductMainImage,
                              product);

                        if (!string.IsNullOrEmpty(product.PriceStr))
                            product.Price = decimal.Round((decimal)product.PriceStr.Replace(",", ".").ToDouble(), 2, MidpointRounding.AwayFromZero);
                        if (!string.IsNullOrEmpty(product.DiscountStr))
                            product.Discount = decimal.Round((decimal)product.DiscountStr.Replace(",", ".").ToDouble(), 2, MidpointRounding.AwayFromZero);

                        product.Lang = CurrentLanguage;
                        await ProductService.SaveOrEditEntityAsync(product);
                        contentId = product.Id;

                        await ProductService.SaveProductTagsAsync(product.Id, tags);

                        if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return RedirectToAction("Index");
                        }
                        else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                        {
                            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, product);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            ViewBag.ProductCategoryTree = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            ViewBag.ProductCategory = await ProductCategoryService.GetSingleAsync(product.ProductCategoryId);
            if (product.MainImageId.HasValue)
            {
                product.MainImage = await FileStorageService.GetSingleAsync(product.MainImageId.Value);
            }
            ViewBag.IsProductPriceEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductPriceEnable);
            product = contentId == 0 ? product : await ProductService.GetBaseContentAsync(contentId, cancellationToken);

            ViewBag.Brands = await GetBrandsSelectListAsync();
            RemoveModelState();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = await ProductService.GetSingleAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            try
            {
                var deleteResult = await ProductService.DeleteProductByIdAsync(id, cancellationToken);
                switch (deleteResult)
                {
                    case ProductDeleteResult.Deleted:
                        SetSuccessMessage();
                        break;
                    case ProductDeleteResult.BlockedByOrders:
                        Logger.Info("Product has sold items cannot be deleted right now. ProductId: " + id);
                        SetErrorMessage(AdminResource.ProductDeleteBlockedByOrders);
                        break;
                    default:
                        SetErrorMessage();
                        break;
                }
                return ReturnIndexIfNotUrlReferrer("Index", new { id = product.ProductCategoryId });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, product);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index", new { id = product.ProductCategoryId });
            }
        }

        [HttpGet]
        public ActionResult Media(int id)
        {
            return RedirectToAction("Index", "Media", new { contentId = id, mod = MediaModType.Products, imageType = EImeceImageType.ProductGallery });
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync(CancellationToken cancellationToken, string format = "excel")
        {
            return await DownloadFileAsync(format, cancellationToken);
        }

        private async Task<ActionResult> DownloadFileAsync(string format, CancellationToken cancellationToken)
        {
            var products = await ProductService.GetAdminPageListAsync(-1, "", CurrentLanguage, cancellationToken);

            var result = from r in products
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             ProductCategory = r.ProductCategory.Name,
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                             Description = r.Description,
                             MainPage = r.MainPage,
                             ImageState = r.ImageState,
                             MainImageId = r.MainImageId,
                             Price = r.Price,
                             Discount = r.Discount,
                             ProductCode = r.ProductCode.ToStr(250),
                             VideoUrl = r.VideoUrl.ToStr(250)
                         };

            return DownloadFile(result, String.Format("Products-{0}", GetCurrentLanguage), format);
        }

        public async Task<ActionResult> MoveProductsInTrees(CancellationToken cancellationToken, int id = 0, string productIdList = "", int oldCategoryId = 0)
        {
            ViewBag.ProductCategoryTreeLeft = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            ViewBag.ProductCategoryTreeRight = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            var products = new System.Collections.Generic.List<Product>();
            if (id > 0)
            {
                products = await ProductService.GetAdminPageListAsync(id, "", CurrentLanguage, cancellationToken);
            }

            var newCategory = await ProductCategoryService.GetSingleAsync(id);
            ViewBag.SelectedCategory = newCategory;

            if (id > 0 && oldCategoryId > 0)
            {
                var oldCategory = await ProductCategoryService.GetSingleAsync(oldCategoryId);
                ViewBag.MoveProductsMessage = String.Format("Seçilen {0} Ürün '{1}' kategorisinden '{2}' kategorisine tasindi", productIdList.Split(',').Count().ToString(), oldCategory.Name, newCategory.Name);
            }

            return View(products);
        }

        public async Task<ActionResult> MoveProducts(CancellationToken cancellationToken, int id, string productIdList, int oldCategoryId)
        {
            await ProductService.MoveProductsInTreesAsync(id, productIdList, cancellationToken);
            return RedirectToAction("MoveProductsInTrees", new { id, productIdList, oldCategoryId });
        }

        private async Task<List<SelectListItem>> GetBrandsSelectListAsync()
        {
            var tagCategories = (await BrandService.GetAllAsync()).Where(r => r.IsActive && r.Lang == CurrentLanguage).OrderBy(r => r.Position).ToList();
            return tagCategories.Select(r => new SelectListItem()
            {
                Text = r.Name.ToStr(),
                Value = r.Id.ToStr()
            }).ToList();
        }
    }
}
