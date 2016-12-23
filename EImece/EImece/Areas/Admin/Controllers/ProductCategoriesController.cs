using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using NLog;
using SharkDev.Web.Controls.TreeView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var productCategories = ProductCategoryRepository.GetAll();
            if (!String.IsNullOrEmpty(search))
            {
                productCategories = productCategories.Where(r => r.Name.ToLower().Contains(search.Trim().ToLower()));
            }
            ViewBag.Tree = CreateProductCategoryTreeViewDataList();
            return View(productCategories.OrderBy(r => r.Position).ThenByDescending(r => r.Id).ToList());
        }


        //
        // GET: /ProductCategory/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductCategory content = ProductCategoryRepository.GetSingle(id);
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
            ViewBag.Tree = CreateProductCategoryTreeViewDataList();

            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
                content.ParentId = 0;
            }
            else
            {

                content = ProductCategoryRepository.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
                parentCategory = ProductCategoryRepository.GetSingle(content.ParentId);


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

                ViewBag.Tree = CreateProductCategoryTreeViewDataList();
                if (ModelState.IsValid)
                {



                    if (productCategoryImage != null)
                    {
                        var mainImage = ImageHelper.SaveFileFromHttpPostedFileBase(productCategoryImage, 0, 0, EImeceImageType.ProductCategoryMainImage);
                        FileStorageRepository.SaveOrEdit(mainImage);
                        productCategory.MainImageId = mainImage.Id;

                    }

                    ProductCategoryRepository.SaveOrEdit(productCategory);
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

            ProductCategory content = ProductCategoryRepository.GetSingle(id);
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

            ProductCategory productCategory = ProductCategoryRepository.GetSingle(id);
            if (productCategory == null)
            {
                return HttpNotFound();
            }
            try
            {
                ProductCategoryRepository.DeleteItem(productCategory);
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
            List<ProductCategory> treelist = ProductCategoryRepository.BuildTree();
            return new JsonResult { Data = new { treeList = treelist }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


    }
}