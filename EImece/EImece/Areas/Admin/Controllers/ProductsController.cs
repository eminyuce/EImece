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
    [Authorize]
    public class ProductsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            var products = ProductRepository.GetAll().ToList();
            if (!String.IsNullOrEmpty(search))
            {
                products = products.Where(r => r.Name.ToLower().Contains(search)).ToList();
            }
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

            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
            }
            else
            {

                content = ProductRepository.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
            }

            return View(content);
        }

        //
        // POST: /Product/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Product product)
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


                    if (product.Id == 0)
                    {
                        ProductRepository.Add(product);
                    }
                    else
                    {
                        ProductRepository.Edit(product);
                    }

                    ProductRepository.Save();
                    int contentId = product.Id;
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
                ProductRepository.Delete(product);
                ProductRepository.Save();
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