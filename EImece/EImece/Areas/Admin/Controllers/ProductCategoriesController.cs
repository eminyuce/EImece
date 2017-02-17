using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using NLog;
using SharkDev.Web.Controls.TreeView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
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
            Expression<Func<ProductCategory, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var productCategories = ProductCategoryService.SearchEntities(whereLambda, search, CurrentLanguage);
            ViewBag.ProductCategoryLeaves = ProductCategoryService.GetProductCategoryLeaves(null, CurrentLanguage);
            return View(productCategories);
        }


        //
        // GET: /ProductCategory/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductCategory content = ProductCategoryService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }
        private List<SelectListItem> GetTemplatesDropDown()
        {
            var templates = TemplateService.GetActiveBaseEntities(true, CurrentLanguage);

            var resultListItem = new List<SelectListItem>();
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

        public ActionResult SaveOrEdit(ProductCategory productCategory, HttpPostedFileBase productCategoryImage = null)
        {
            try
            {


                if (ModelState.IsValid)
                {

                    FilesHelper.SaveFileFromHttpPostedFileBase(productCategoryImage,
                        productCategory.ImageHeight,
                        productCategory.ImageWidth,
                        EImeceImageType.ProductCategoryMainImage,
                        productCategory);

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
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList(CurrentLanguage);
            ViewBag.Templates = GetTemplatesDropDown();
            return View(productCategory);
        }



        //
        // GET: /ProductCategory/Delete/5
        [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductCategory content = ProductCategoryService.GetSingle(id);
            var leaves = ProductCategoryService.GetProductCategoryLeaves(null, CurrentLanguage);
            if (content == null)
            {
                return HttpNotFound();
            }

            if (leaves.Any(r => r.Id == id))
            {
                return View(content);
            }
            else
            {
                return Content("You cannot delete the parent");
            }

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
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(productCategory);

        }
       

    }
}