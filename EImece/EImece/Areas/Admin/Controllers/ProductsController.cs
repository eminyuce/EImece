using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using EImece.Web.Areas.Admin.Controllers;
using EImece.Web.Filters;
using EImece.Web.Helpers;
using Microsoft.Extensions.Logging;
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
        private const string IndexAction = "Index";

        protected IProductService ProductService { get; }
        protected IProductCategoryService ProductCategoryService { get; }
        protected IBrandService BrandService { get; }
        protected ITagService TagService { get; }
        protected ITagCategoryService TagCategoryService { get; }
        protected ITemplateService TemplateService { get; }
        protected IFileStorageService FileStorageService { get; }
        protected IEntityFactory EntityFactory { get; }
        protected FilesHelper FilesHelper { get; }

        public ProductsController(ISettingService settingService,
            IProductService productService,
            IProductCategoryService productCategoryService,
            IBrandService brandService,
            ITagService tagService,
            ITagCategoryService tagCategoryService,
            ITemplateService templateService,
            IFileStorageService fileStorageService,
            IEntityFactory entityFactory,
            FilesHelper filesHelper, ILogger<ProductsController> logger)
            : base(settingService, logger)
        {
            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
            ProductCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
            BrandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
            TagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            TagCategoryService = tagCategoryService ?? throw new ArgumentNullException(nameof(tagCategoryService));
            TemplateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            FileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
            FilesHelper = filesHelper ?? throw new ArgumentNullException(nameof(filesHelper));
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(
            int id = 0,
            AdminProductsIndexQuery query = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (query == null)
            {
                query = new AdminProductsIndexQuery();
            }

            if (!CanRenderGrid())
            {
                string redirectSearch = !string.IsNullOrWhiteSpace(query.Search) ? query.Search : (!string.IsNullOrWhiteSpace(query.Name) ? query.Name : null);
                return RedirectToAction(IndexAction, "Products", new
                {
                    id = id,
                    area = "Admin",
                    search = redirectSearch,
                    brandId = query.BrandId > 0 ? (int?)query.BrandId : null,
                    state = string.IsNullOrEmpty(query.State) ? null : query.State,
                    isActive = query.IsActive,
                    mainPage = query.MainPage,
                    isCampaign = query.IsCampaign,
                    minPrice = query.MinPrice,
                    maxPrice = query.MaxPrice
                });
            }

            var isProductPriceEnable = await SettingService.GetSettingObjectByKeyFromDbAsync(Constants.IsProductPriceEnable).ConfigureAwait(false);
            bool priceEnabled = isProductPriceEnable == null || isProductPriceEnable.SettingValue.ToBool(true);

            var filter = new ProductAdminListFilter
            {
                State = query.State,
                IsActive = query.IsActive == true ? true : (bool?)null,
                MainPage = query.MainPage == true ? true : (bool?)null,
                IsCampaign = query.IsCampaign == true ? true : (bool?)null,
                MinPrice = query.MinPrice,
                MaxPrice = query.MaxPrice,
                ApplyPriceFilter = priceEnabled
            };

            string searchParam = !string.IsNullOrWhiteSpace(query.Search) ? query.Search : (!string.IsNullOrWhiteSpace(query.Name) ? query.Name : null);
            var products = await ProductService.GetAdminPageListAsync(id, query.BrandId, searchParam, CurrentLanguage, filter, cancellationToken).ConfigureAwait(false);
            ViewBag.IsProductPriceEnable = isProductPriceEnable;
            ViewBag.PriceEnabled = priceEnabled;
            return AdminGridResult(products);
        }

        [HttpGet]
        public async Task<ActionResult> Index(
            CancellationToken cancellationToken,
            int id = 0,
            AdminProductsIndexQuery query = null)
        {
            if (query == null)
            {
                query = new AdminProductsIndexQuery();
            }

            var isProductPriceEnable = await SettingService.GetSettingObjectByKeyFromDbAsync(Constants.IsProductPriceEnable);
            bool priceEnabled = isProductPriceEnable == null || isProductPriceEnable.SettingValue.ToBool(true);

            var filter = new ProductAdminListFilter
            {
                State = query.State,
                IsActive = query.IsActive == true ? true : (bool?)null,
                MainPage = query.MainPage == true ? true : (bool?)null,
                IsCampaign = query.IsCampaign == true ? true : (bool?)null,
                MinPrice = query.MinPrice,
                MaxPrice = query.MaxPrice,
                ApplyPriceFilter = priceEnabled
            };

            ViewBag.ProductCategoryTree = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            var products = await ProductService.GetAdminPageListAsync(id, query.BrandId, query.Search, CurrentLanguage, filter, cancellationToken);
            ViewBag.IsProductPriceEnable = isProductPriceEnable;
            ViewBag.SelectedCategory = await ProductCategoryService.GetSingleAsync(id);
            ViewBag.SelectedBrandId = query.BrandId;
            ViewBag.SelectedState = query.State.ToStr();
            ViewBag.FilterIsActive = filter.IsActive;
            ViewBag.FilterMainPage = filter.MainPage;
            ViewBag.FilterIsCampaign = filter.IsCampaign;
            ViewBag.MinPrice = priceEnabled ? query.MinPrice : null;
            ViewBag.MaxPrice = priceEnabled ? query.MaxPrice : null;
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

            Product content = await ProductService.GetProductByIdAsync(id, cancellationToken);
            if (content == null)
            {
                return HttpNotFound();
            }

            Template template = null;
            try
            {
                template = await ResolveProductTemplateAsync(content, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to resolve product XML template for product {ProductId}", id);
            }

            if (template == null || string.IsNullOrWhiteSpace(template.TemplateXml))
            {
                TempData["ErrorMessage"] = "Bu \u00fcr\u00fcn\u00fcn kategorisine ba\u011fl\u0131 ge\u00e7erli bir \u00fcr\u00fcn XML \u015fablonu yok. \u00d6nce kategoriye \u015fablon atay\u0131n.";
                if (content.ProductCategoryId > 0)
                {
                    return RedirectToAction(IndexAction, new { id = content.ProductCategoryId });
                }
                return RedirectToAction(IndexAction);
            }

            ViewBag.Template = template;
            return View(content);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEditProductSpecs(CancellationToken cancellationToken, int id, int templateId, String saveButton = null)
        {
            int productId = id;
            var savedOk = false;
            try
            {
                if (templateId <= 0)
                {
                    ModelState.AddModelError("", "ÃœrÃ¼n kategorisine baÄŸlÄ± geÃ§erli bir ÅŸablon yok. Ã–nce kategoriye ÅŸablon atayÄ±n.");
                }
                else
                {
                    await ProductService.ParseTemplateAndSaveProductSpecificationsAsync(productId, templateId, CurrentLanguage, Request.Unvalidated.Form, cancellationToken);
                    savedOk = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to save product specifications for product {ProductId}, template {TemplateId}", productId, templateId);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
            }

            if (savedOk
                && !String.IsNullOrEmpty(saveButton)
                && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
            {
                return RedirectToAction(IndexAction);
            }

            if (savedOk)
            {
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            }

            Product content = await ProductService.GetProductByIdAsync(id, cancellationToken);
            if (content == null)
            {
                return HttpNotFound();
            }
            ViewBag.Template = await ResolveProductTemplateAsync(content, cancellationToken);
            RemoveModelState();
            return View(content);
        }

        private async Task<Template> ResolveProductTemplateAsync(Product content, CancellationToken cancellationToken)
        {
            if (content == null)
            {
                return null;
            }

            int? templateId = null;
            if (content.ProductCategory != null
                && content.ProductCategory.TemplateId.HasValue
                && content.ProductCategory.TemplateId.Value > 0)
            {
                templateId = content.ProductCategory.TemplateId.Value;
            }

            if (!templateId.HasValue)
            {
                return null;
            }

            return await TemplateService.GetTemplateAsync(templateId.Value, cancellationToken);
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
                if (content == null)
                {
                    return HttpNotFound();
                }
                content.PriceStr = decimal.Round(content.Price, 2, MidpointRounding.AwayFromZero).ToString().Replace(".", ",");
                content.DiscountStr = content.Discount.HasValue ? decimal.Round(content.Discount.Value, 2, MidpointRounding.AwayFromZero).ToString().Replace(".", ",") : "";
                productCategory = await ProductCategoryService.GetSingleAsync(content.ProductCategoryId);
            }
            ViewBag.ProductCategory = productCategory;
            ViewBag.IsProductPriceEnable = await SettingService.GetSettingObjectByKeyFromDbAsync(Constants.IsProductPriceEnable);
            return View(content);
        }

        //
        // POST: /Product/Create

        [HttpPost, ValidateInput(false)]
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
                        var isEdit = product.Id > 0;

                        FilesHelper.SaveFileFromHttpPostedFileBase(
                             postedImage,
                             product.ImageHeight,
                             product.ImageWidth,
                             EImeceImageType.ProductMainImage,
                              product);

                        ApplyPostedProductPrices(product);

                        product.Lang = CurrentLanguage;
                        await ProductService.SaveOrEditEntityAsync(product);
                        contentId = product.Id;

                        await ProductService.SaveProductTagsAsync(product.Id, tags);

                        if (isEdit || (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            return RedirectToProductIndex(product.ProductCategoryId);
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
                Logger.LogError(ex, "Unable to save changes:" + ex.StackTrace, product);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            ViewBag.ProductCategoryTree = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            ViewBag.ProductCategory = await ProductCategoryService.GetSingleAsync(product.ProductCategoryId);
            if (product.MainImageId.HasValue)
            {
                product.MainImage = await FileStorageService.GetSingleAsync(product.MainImageId.Value);
            }
            ViewBag.IsProductPriceEnable = await SettingService.GetSettingObjectByKeyFromDbAsync(Constants.IsProductPriceEnable);
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
                        Logger.LogInformation("Product has sold items cannot be deleted right now. ProductId: " + id);
                        SetErrorMessage(AdminResource.ProductDeleteBlockedByOrders);
                        break;
                    default:
                        SetErrorMessage();
                        break;
                }
                return ReturnIndexIfNotUrlReferrer(IndexAction, new { id = product.ProductCategoryId });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to delete product:" + ex.StackTrace, product);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer(IndexAction, new { id = product.ProductCategoryId });
            }
        }

        [HttpGet]
        public ActionResult Media(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return RedirectToAction(IndexAction);
            }

            return RedirectToAction(IndexAction, "Media", new { contentId = id.Value, mod = MediaModType.Products, imageType = EImeceImageType.ProductGallery });
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
                             Description = (r.Description ?? string.Empty).ToStr(32767),
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

        public async Task<ActionResult> MoveProductsInTrees(CancellationToken cancellationToken, int id = 0, string search = "", string productIdList = "", int oldCategoryId = 0)
        {
            ViewBag.ProductCategoryTreeLeft = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            ViewBag.ProductCategoryTreeRight = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            ViewBag.Search = search;
            var products = new System.Collections.Generic.List<Product>();
            if (id > 0)
            {
                products = await ProductService.GetAdminPageListAsync(id, search, CurrentLanguage, cancellationToken);
            }

            var newCategory = await ProductCategoryService.GetSingleAsync(id);
            ViewBag.SelectedCategory = newCategory;

            if (id > 0 && oldCategoryId > 0)
            {
                var oldCategory = await ProductCategoryService.GetSingleAsync(oldCategoryId);
                var oldCatName = oldCategory != null ? oldCategory.Name : "-";
                var newCatName = newCategory != null ? newCategory.Name : "-";
                var count = productIdList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
                ViewBag.MoveProductsMessage = String.Format("SeÃ§ilen {0} Ã¼rÃ¼n '{1}' kategorisinden '{2}' kategorisine baÅŸarÄ±yla taÅŸÄ±ndÄ±.", count, oldCatName, newCatName);
            }

            return View(products);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> MoveProductsInTreesGrid(CancellationToken cancellationToken, int id = 0, string search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("MoveProductsInTrees", new { id, search });
            }

            var products = new System.Collections.Generic.List<Product>();
            if (id > 0)
            {
                products = await ProductService.GetAdminPageListAsync(id, search, CurrentLanguage, cancellationToken);
            }

            return AdminGridResult(products);
        }

        public async Task<ActionResult> MoveProducts(CancellationToken cancellationToken, int? id, string productIdList, int? oldCategoryId)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return RedirectToAction("MoveProductsInTrees");
            }

            var categoryId = id.Value;
            await ProductService.MoveProductsInTreesAsync(categoryId, productIdList, cancellationToken);
            return RedirectToAction("MoveProductsInTrees", new { id = categoryId, productIdList, oldCategoryId = oldCategoryId.GetValueOrDefault() });
        }

        private async Task<List<SelectListItem>> GetBrandsSelectListAsync()
        {
            var brands = (await BrandService.GetAllAsync()).Where(r => r.IsActive && r.Lang == CurrentLanguage).ToList();
            // Sort alphabetically by name for searchable combobox; fallback to Position for stable order when names equal.
            return brands.OrderBy(r => r.Name.ToStr(), StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"), true))
                .ThenBy(r => r.Position)
                .Select(r => new SelectListItem()
                {
                    Text = r.Name.ToStr(),
                    Value = r.Id.ToStr()
                }).ToList();
        }

        private ActionResult RedirectToProductIndex(int productCategoryId)
        {
            return productCategoryId > 0
                ? RedirectToAction(IndexAction, new { id = productCategoryId })
                : RedirectToAction(IndexAction);
        }

        private static void ApplyPostedProductPrices(Product product)
        {
            if (!string.IsNullOrEmpty(product.PriceStr))
            {
                product.Price = decimal.Round((decimal)product.PriceStr.Replace(",", ".").ToDouble(), 2, MidpointRounding.AwayFromZero);
            }
            if (!string.IsNullOrEmpty(product.DiscountStr))
            {
                product.Discount = decimal.Round((decimal)product.DiscountStr.Replace(",", ".").ToDouble(), 2, MidpointRounding.AwayFromZero);
            }
        }
    }

    /// <summary>
    /// Query-string filters for Admin Products Index (keeps the action under S107 parameter limits).
    /// Property names match the previous action parameters so MVC binding is unchanged.
    /// </summary>
    public sealed class AdminProductsIndexQuery
    {
        public int BrandId { get; set; } = -1;

        public string Search { get; set; } = "";

        public string Name { get; set; } = "";

        public string State { get; set; } = "";

        public bool? IsActive { get; set; }

        public bool? MainPage { get; set; }

        public bool? IsCampaign { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }
    }
}
