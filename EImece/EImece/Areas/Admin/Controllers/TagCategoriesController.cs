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
    public class TagCategoriesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            var tags = TagCategoryRepository.GetAll();
            if (!String.IsNullOrEmpty(search))
            {
                tags = tags.Where(r => r.Name.ToLower().Contains(search.Trim().ToLower()));
            }
            return View(tags.OrderBy(r => r.Position).ToList());
        }


 
        //
        // GET: /TagCategory/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = new TagCategory();


            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
            }
            else
            {
                content = TagCategoryRepository.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
            }


            return View(content);
        }

        //
        // POST: /TagCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(TagCategory TagCategory)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    TagCategoryRepository.SaveOrEdit(TagCategory);
                    int contentId = TagCategory.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, TagCategory);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(TagCategory);
        }



        //
        // GET: /TagCategory/Delete/5
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TagCategory content = TagCategoryRepository.GetSingle(id);
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

            TagCategory TagCategory = TagCategoryRepository.GetSingle(id);
            if (TagCategory == null)
            {
                return HttpNotFound();
            }
            try
            {
                TagCategoryRepository.DeleteItem(TagCategory);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, TagCategory);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(TagCategory);

        }
    }
}