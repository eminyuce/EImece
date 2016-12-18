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
    public class StoriesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            var categories = StoryRepository.GetAll().ToList();
            if (!String.IsNullOrEmpty(search))
            {
                categories = categories.Where(r => r.Name.ToLower().Contains(search)).ToList();
            }
            return View(categories);
        }


        //
        // GET: /Story/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Story content = StoryRepository.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }

        //
        // GET: /Story/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = new Story();


            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
            }
            else
            {
                content = StoryRepository.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
            }


            return View(content);
        }

        //
        // POST: /Story/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Story Story)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    StoryRepository.SaveOrEdit(Story);
                    int contentId = Story.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, Story);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(Story);
        }



        //
        // GET: /Story/Delete/5
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Story content = StoryRepository.GetSingle(id);
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

            Story Story = StoryRepository.GetSingle(id);
            if (Story == null)
            {
                return HttpNotFound();
            }
            try
            {
                StoryRepository.DeleteItem(Story);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, Story);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(Story);

        }
    }
}