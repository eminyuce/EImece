using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
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
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList();
            Expression<Func<ProductCategory, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var productCategories = ProductCategoryService.SearchEntities(whereLambda, search);
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

        //
        // GET: /ProductCategory/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = new ProductCategory();
            var parentCategory = new ProductCategory();
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList();

            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
                content.ParentId = 0;
            }
            else
            {

                content = ProductCategoryService.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
                parentCategory = ProductCategoryService.GetSingle(content.ParentId);


            }
            ViewBag.ParentCategory = parentCategory;
            return View(content);
        }

        //
        // POST: /ProductCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(ProductCategory productCategory, HttpPostedFileBase productCategoryImage=null)
        {
            try
            {

   
                if (ModelState.IsValid)
                {

                    if (productCategoryImage != null)
                    {
                        var mainImage = ImageHelper.SaveFileFromHttpPostedFileBase(productCategoryImage, 0, 0, EImeceImageType.ProductCategoryMainImage);
                        FileStorageService.SaveOrEditEntity(mainImage);
                        productCategory.MainImageId = mainImage.Id;
                    }

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
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList();
            return View(productCategory);
        }



        //
        // GET: /ProductCategory/Delete/5
        public ActionResult Delete(int id = 0)
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
                ProductCategoryService.DeleteEntity(productCategory);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, productCategory);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(productCategory);

        }
        public ActionResult GetCategories()
        {
            List<ProductCategory> treelist = ProductCategoryService.BuildTree();
            return new JsonResult { Data = new { treeList = treelist }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


    }
}