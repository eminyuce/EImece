using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ProductCategoriesController : BaseAdminController
    {
        // GET: Admin/ProductCategories
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index(String search = "")
        {
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList(CurrentLanguage);
            var productCategories = ProductCategoryService.GetAdminProductCategories(search, CurrentLanguage);
            ViewBag.ProductCategoryLeaves = ProductCategoryService.GetProductCategoryLeaves(null, CurrentLanguage);
            return View(productCategories);
        }

        private List<SelectListItem> GetTemplatesDropDown()
        {
            var templates = TemplateService.GetActiveBaseEntities(true, CurrentLanguage);

            var resultListItem = new List<SelectListItem>();
            resultListItem.Add(new SelectListItem() { Text = "Select Template", Value = "0" });
            foreach (var item in templates)
            {
                resultListItem.Add(new SelectListItem() { Text = item.Name, Value = item.Id.ToStr() });
            }
            return resultListItem;
        }

        //
        // GET: /ProductCategory/Create

        public ActionResult SaveOrEdit(int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<ProductCategory>();
            var parentCategory = EntityFactory.GetBaseContentInstance<ProductCategory>();
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList(CurrentLanguage);
            ViewBag.Templates = GetTemplatesDropDown();
            if (id == 0)
            {
                content.ParentId = 0;
            }
            else
            {
                content = ProductCategoryService.GetBaseContent(id);
                parentCategory = ProductCategoryService.GetSingle(content.ParentId);
            }
            ViewBag.ParentCategory = parentCategory;
            return View(content);
        }

        //
        // POST: /ProductCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(ProductCategory productCategory, HttpPostedFileBase postedImage = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(postedImage,
                        productCategory.ImageHeight,
                        productCategory.ImageWidth,
                        EImeceImageType.ProductCategoryMainImage,
                        productCategory);
                    if (!productCategory.TemplateId.HasValue)
                    {
                        productCategory.TemplateId = 0;
                    }
                    productCategory.Lang = CurrentLanguage;
                    ProductCategoryService.SaveOrEditEntity(productCategory);
                    int contentId = productCategory.Id;

                    return RedirectToAction("Index");
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, productCategory);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList(CurrentLanguage);
            ViewBag.Templates = GetTemplatesDropDown();
            return View(productCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductCategory productCategory = ProductCategoryService.GetSingle(id);
            if (productCategory == null)
            {
                return HttpNotFound();
            }
            try
            {
                ProductCategoryService.DeleteProductCategory(productCategory.Id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, productCategory);
                ModelState.AddModelError("*", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return View(productCategory);
        }

        public ActionResult ExportExcel()
        {
            String search = "";
            Expression<Func<ProductCategory, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var productCategories = ProductCategoryService.SearchEntities(whereLambda, search, CurrentLanguage);

            var result = from r in productCategories
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             ParentId = r.ParentId.ToStr(250),
                             Name = r.Name.ToStr(250),
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                             Description = r.Description,
                             MainPage = r.MainPage.ToStr(250),
                             ImageState = r.ImageState.ToStr(250),
                             MainImageId = r.MainImageId.ToStr(250)
                         };

            return DownloadFile(result, String.Format("ProductCategories-{0}", GetCurrentLanguage));
        }
    }
}