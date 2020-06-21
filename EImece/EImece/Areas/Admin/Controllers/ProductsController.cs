using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ProductsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index(int id = 0, String search = "")
        {
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList(CurrentLanguage);
            var products = ProductService.GetAdminPageList(id, search, CurrentLanguage);
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey(ApplicationConfigs.IsProductPriceEnable);
            ViewBag.SelectedCategory = ProductCategoryService.GetSingle(id);
            return View(products);
        }

        public ActionResult SaveOrEditProductSpecs(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var productDetailViewModel = ProductService.GetProductById(id);
            Product content = productDetailViewModel.Product;
            ViewBag.Template = TemplateService.GetTemplate(content.ProductCategory.TemplateId.Value);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }

        [HttpPost]
        public ActionResult SaveOrEditProductSpecs(int id, int templateId)
        {
            int productId = id;
            ProductService.ParseTemplateAndSaveProductSpecifications(productId, templateId, CurrentLanguage, Request);
            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            var productDetailViewModel = ProductService.GetProductById(id);
            Product content = productDetailViewModel.Product;
            ViewBag.Template = TemplateService.GetTemplate(content.ProductCategory.TemplateId.Value);
            return View(content);
            //  return RedirectToAction("SaveOrEditProductSpecs", new { id });
        }

        //
        // GET: /Product/Create

        public ActionResult SaveOrEdit(int id = 0)
        {
            TempData[TempDataReturnUrlReferrer] = Request.UrlReferrer.ToStr();
            var content = EntityFactory.GetBaseContentInstance<Product>();
            var productCategory = EntityFactory.GetBaseContentInstance<ProductCategory>();
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList(CurrentLanguage);

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
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey(ApplicationConfigs.IsProductPriceEnable);
            return View(content);
        }

        //
        // POST: /Product/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Product product, int[] tags = null, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            int contentId = 0;
            try
            {
                if (ModelState.IsValid)
                {
                    if (product.ProductCategoryId == 0)
                    {
                        ModelState.AddModelError("ProductCategoryId", AdminResource.ProductCategoryIdErrorMessage);
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

                        if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText))
                        {
                            return ReturnTempUrl("Index");
                        }
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, product);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList(CurrentLanguage);
            ViewBag.ProductCategory = ProductCategoryService.GetSingle(product.ProductCategoryId);
            if (product.MainImageId.HasValue)
            {
                product.MainImage = FileStorageService.GetSingle(product.MainImageId.Value);
            }
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey(ApplicationConfigs.IsProductPriceEnable);
            product = contentId == 0 ? product : ProductService.GetBaseContent(contentId);
            if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonText))
            {
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = ProductService.GetSingle(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            try
            {
                ProductService.DeleteProductById(id);
                return RedirectToAction("Index", new { categoryId = product.ProductCategoryId });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, product);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return View(product);
        }

        public ActionResult Media(int id)
        {
            return RedirectToAction("Index", "Media", new { contentId = id, mod = MediaModType.Products, imageType = EImeceImageType.ProductGallery });
        }

        public ActionResult ExportExcel()
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
    }
}