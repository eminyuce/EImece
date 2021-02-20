using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ProductsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public ActionResult Index(int id = 0, String search = "")
        {
            ViewBag.ProductCategoryTree = ProductCategoryService.BuildTree(null, CurrentLanguage);
            var products = ProductService.GetAdminPageList(id, search, CurrentLanguage);
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey(Constants.IsProductPriceEnable);
            ViewBag.SelectedCategory = ProductCategoryService.GetSingle(id);
            return View(products);
        }

        [HttpGet]
        public ActionResult SaveOrEditProductSpecs(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var productDetailViewModel = ProductService.GetProductDetailViewModelById(id);
            Product content = productDetailViewModel.Product;
            ViewBag.Template = TemplateService.GetTemplate(content.ProductCategory.TemplateId.Value);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult SaveOrEditProductSpecs(int id, int templateId)
        {
            int productId = id;
            ProductService.ParseTemplateAndSaveProductSpecifications(productId, templateId, CurrentLanguage, Request);
            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            var productDetailViewModel = ProductService.GetProductDetailViewModelById(id);
            Product content = productDetailViewModel.Product;
            ViewBag.Template = TemplateService.GetTemplate(content.ProductCategory.TemplateId.Value);
            RemoveModelState();
            return View(content);
        }
        //
        // GET: /Product/Create
        [HttpGet]
        public ActionResult SaveOrEdit(int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<Product>();
            ViewBag.Brands = GetBrandsSelectList();
            var productCategory = EntityFactory.GetBaseContentInstance<ProductCategory>();
            ViewBag.ProductCategoryTree = ProductCategoryService.BuildTree(null, CurrentLanguage);
            if (id == 0)
            {
                content.ProductCategoryId = 0;
            }
            else
            {
                content = ProductService.GetBaseContent(id);
                productCategory = ProductCategoryService.GetSingle(content.ProductCategoryId);
            }
            ViewBag.ProductCategory = productCategory;
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey(Constants.IsProductPriceEnable);
            return View(content);
        }

        //
        // POST: /Product/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Product product, int[] tags = null, HttpPostedFileBase postedImage = null, String saveButton = null)
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
                    var isProductPriceEnable = SettingService.GetSettingObjectByKey(Constants.IsProductPriceEnable);
                    if (product.ProductCategoryId == 0)
                    {
                        ModelState.AddModelError("ProductCategoryId", AdminResource.ProductCategoryIdErrorMessage);
                        ModelState.AddModelError("", AdminResource.ProductCategoryIdErrorMessage);
                    }
                    else if (isProductPriceEnable.SettingValue.ToBool(false) && product.Price <= 0)
                    {
                        ModelState.AddModelError("Price", AdminResource.PriceErrorMessage);
                        ModelState.AddModelError("", AdminResource.PriceErrorMessage);
                    }
                    else
                    {
                        FilesHelper.SaveFileFromHttpPostedFileBase(
                             postedImage,
                             product.ImageHeight,
                             product.ImageWidth,
                             EImeceImageType.ProductMainImage,
                              product);

                        product.Lang = CurrentLanguage;
                        ProductService.SaveOrEditEntity(product);
                        contentId = product.Id;

                        ProductService.SaveProductTags(product.Id, tags);

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
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, product);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            ViewBag.ProductCategoryTree = ProductCategoryService.BuildTree(null, CurrentLanguage);
            ViewBag.ProductCategory = ProductCategoryService.GetSingle(product.ProductCategoryId);
            if (product.MainImageId.HasValue)
            {
                product.MainImage = FileStorageService.GetSingle(product.MainImageId.Value);
            }
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey(Constants.IsProductPriceEnable);
            product = contentId == 0 ? product : ProductService.GetBaseContent(contentId);

            ViewBag.Brands = GetBrandsSelectList();
            RemoveModelState();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = ProductService.GetSingle(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            try
            {
                ProductService.DeleteProductById(id);
                return ReturnIndexIfNotUrlReferrer("Index", new { id = product.ProductCategoryId });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, product);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        public ActionResult Media(int id)
        {
            return RedirectToAction("Index", "Media", new { contentId = id, mod = MediaModType.Products, imageType = EImeceImageType.ProductGallery });
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync()
        {
            return await Task.Run(() =>
            {
                return DownloadFile();
            }).ConfigureAwait(true);
        }

        private ActionResult DownloadFile()
        {
            var products = ProductService.GetAdminPageList(-1, "", CurrentLanguage);

            var result = from r in products
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             ProductCategory = r.ProductCategory.Name,
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                             Description = r.Description,
                             MainPage = r.MainPage.ToStr(250),
                             ImageState = r.ImageState.ToStr(250),
                             MainImageId = r.MainImageId.ToStr(250),
                             Price = r.Price.ToStr(250),
                             Discount = r.Discount.ToStr(250),
                             ProductCode = r.ProductCode.ToStr(250),
                             VideoUrl = r.VideoUrl.ToStr(250)
                         };

            return DownloadFile(result, String.Format("Products-{0}", GetCurrentLanguage));
        }

        public ActionResult MoveProductsInTrees(int id = 0, string productIdList = "", int oldCategoryId = 0)
        {
            ViewBag.ProductCategoryTreeLeft = ProductCategoryService.BuildTree(null, CurrentLanguage);
            ViewBag.ProductCategoryTreeRight = ProductCategoryService.BuildTree(null, CurrentLanguage);
            var products = new System.Collections.Generic.List<Product>();
            if (id > 0)
            {
                products = ProductService.GetAdminPageList(id, "", CurrentLanguage);
            }

            var newCategory = ProductCategoryService.GetSingle(id);
            ViewBag.SelectedCategory = newCategory;

            if (id > 0 && oldCategoryId > 0)
            {
                var oldCategory = ProductCategoryService.GetSingle(oldCategoryId);
                ViewBag.MoveProductsMessage = String.Format("Seçilen {0} Ürün '{1}' kategorisinden '{2}' kategorisine tasindi", productIdList.Split(',').Count().ToString(), oldCategory.Name, newCategory.Name);
            }

            return View(products);
        }

        public ActionResult MoveProducts(int id, string productIdList, int oldCategoryId)
        {
            ProductService.MoveProductsInTrees(id, productIdList);
            return RedirectToAction("MoveProductsInTrees", new { id, productIdList, oldCategoryId });
        }

        private List<SelectListItem> GetBrandsSelectList()
        {
            var tagCategories = BrandService.GetAll().Where(r => r.IsActive && r.Lang == CurrentLanguage).OrderBy(r => r.Position).ToList();
            return tagCategories.Select(r => new SelectListItem()
            {
                Text = r.Name.ToStr(),
                Value = r.Id.ToStr()
            }).ToList();
        }
    }
}