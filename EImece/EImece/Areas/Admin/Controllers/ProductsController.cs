using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    // [Authorize]
    public class ProductsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(int id = 0, String search = "", int lang = 1)
        {
            var products = ProductRepository.GetAdminPageList(id, search, lang);
            ViewBag.Tree = CreateProductCategoryTreeViewDataList();
            return View(products);
        }

        //
        // GET: /Product/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product content = ProductRepository.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }

        //
        // GET: /Product/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = new Product();
            var productCategory = new ProductCategory();
            ViewBag.Tree = CreateProductCategoryTreeViewDataList();

            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
                content.ProductCategoryId = 0;
            }
            else
            {

                content = ProductRepository.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
                productCategory = ProductCategoryRepository.GetSingle(content.ProductCategoryId);
            }
            ViewBag.ProductCategory = productCategory;
            return View(content);
        }

        //
        // POST: /Product/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Product product, int [] tags = null)
        {
            try
            {
         
                if (ModelState.IsValid)
                {

                    if (product.ProductCategoryId == 0)
                    {
                        ModelState.AddModelError("ProductCategoryId", "You should select category from category tree.");
                        return View(product);
                    }


                    ProductRepository.SaveOrEdit(product);
                    int contentId = product.Id;

                    if(tags!= null)
                    {
                        ProductTagRepository.SaveProductTags(product.Id, tags);
                    }
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, product);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(product);
        }



        //
        // GET: /Product/Delete/5
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product content = ProductRepository.GetSingle(id);
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

            Product product = ProductRepository.GetSingle(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            try
            {
                ProductRepository.DeleteItem(product);
                return RedirectToAction("Index", new { categoryId = product.ProductCategoryId });
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