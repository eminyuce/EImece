using EImece.Domain.Entities;
using NLog;
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
            var productCategories = ProductCategoryRepository.GetAll().ToList();
            if (!String.IsNullOrEmpty(search))
            {
                productCategories = productCategories.Where(r => r.Name.ToLower().Contains(search)).ToList();
            }
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

            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
            }
            else
            {

                content = ProductCategoryRepository.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
            }

            return View(content);
        }

        //
        // POST: /ProductCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(ProductCategory productCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (productCategory.Id == 0)
                    {
                        ProductCategoryRepository.Add(productCategory);
                    }
                    else
                    {
                        ProductCategoryRepository.Edit(productCategory);
                    }

                    ProductCategoryRepository.Save();
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

            ProductCategory product = ProductCategoryRepository.GetSingle(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            try
            {
                ProductCategoryRepository.Delete(product);
                ProductCategoryRepository.Save();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, product);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(product);

        }

    }
}