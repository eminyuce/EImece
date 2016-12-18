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
    public class StoryCategoriesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            var categories = StoryCategoryRepository.GetAll().ToList();
            if (!String.IsNullOrEmpty(search))
            {
                categories = categories.Where(r => r.Name.ToLower().Contains(search)).ToList();
            }
            return View(categories);
        }


        //
        // GET: /StoryCategory/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StoryCategory content = StoryCategoryRepository.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }

        //
        // GET: /StoryCategory/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = new StoryCategory();
 

            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
            }
            else
            {
                content = StoryCategoryRepository.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
            }


            return View(content);
        }

        //
        // POST: /StoryCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(StoryCategory StoryCategory)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    StoryCategoryRepository.SaveOrEdit(StoryCategory);
                    int contentId = StoryCategory.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, StoryCategory);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(StoryCategory);
        }



        //
        // GET: /StoryCategory/Delete/5
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StoryCategory content = StoryCategoryRepository.GetSingle(id);
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

            StoryCategory StoryCategory = StoryCategoryRepository.GetSingle(id);
            if (StoryCategory == null)
            {
                return HttpNotFound();
            }
            try
            {
                StoryCategoryRepository.DeleteItem(StoryCategory);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, StoryCategory);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(StoryCategory);

        }
    }
}