using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace EImece.Areas.Admin.Controllers
{
    public class ProductsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public ActionResult Index(int id = 0, String search = "")
        {
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList(CurrentLanguage);
            var products = ProductService.GetAdminPageList(id, search, CurrentLanguage);
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey("IsProductPriceEnable");
            ViewBag.SelectedCategory  = ProductCategoryService.GetSingle(id);
            return View(products);
        }
        public ActionResult SaveOrEditProductSpecs(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product content = ProductService.GetProductById(id).Product;
            ViewBag.Template = TemplateService.GetSingle(content.ProductCategory.TemplateId.Value);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }
        [HttpPost]
        public ActionResult SaveOrEditProductSpecs(int id,int templateId)
        {
            int productId = id;
            ProductService.ParseTemplateAndSaveProductSpecifications(productId,templateId,CurrentLanguage,Request);

            return RedirectToAction("SaveOrEditProductSpecs", new { id });
        }
        //
        // GET: /Product/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product content = ProductService.GetBaseContent(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey("IsProductPriceEnable");
            return View(content);
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
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey(Settings.IsProductPriceEnable);
            return View(content);
        }

        //
        // POST: /Product/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Product product, int[] tags = null, HttpPostedFileBase productImage = null)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    if (product.ProductCategoryId == 0)
                    {
                        ModelState.AddModelError("ProductCategoryId", "You should select category from category tree.");
                    }
                    else
                    {

                       FilesHelper.SaveFileFromHttpPostedFileBase(
                            productImage,
                            product.ImageHeight,
                            product.ImageWidth,
                            EImeceImageType.ProductMainImage,
                             product);

                        product.Lang = CurrentLanguage;
                        ProductService.SaveOrEditEntity(product);
                        int contentId = product.Id;

                            ProductService.SaveProductTags(product.Id, tags);
                        


                        return ReturnTempUrl("Index");
                       
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
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator." + ex.StackTrace);
            }


            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList(CurrentLanguage);
            ViewBag.ProductCategory = ProductCategoryService.GetSingle(product.ProductCategoryId);
            if (product.MainImageId.HasValue)
            {
                product.MainImage = FileStorageService.GetSingle(product.MainImageId.Value);
            }
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey(Settings.IsProductPriceEnable);
            return View(product);
        }



        [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product content = ProductService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }


            return View(content);
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
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
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