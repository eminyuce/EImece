using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class TagsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            var tags = TagRepository.GetAll();
            if (!String.IsNullOrEmpty(search))
            {
                tags = tags.Where(r => r.Name.ToLower().Contains(search.Trim().ToLower()));
            }
            
            return View(tags.OrderBy(r => r.Position).ToList());
        }


        //
        // GET: /Tag/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tag content = TagRepository.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }

        //
        // GET: /Tag/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = new Tag();
            ViewBag.Categories = TagCategoryRepository.GetAll().OrderBy(r=>r.Position).ToList().Select(r=> new SelectListItem() { Text = r.Name.ToStr(), Value = r.Id.ToStr() }).ToList();

            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
            }
            else
            {
                content = TagRepository.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
            }


            return View(content);
        }

        //
        // POST: /Tag/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Tag Tag)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    TagRepository.SaveOrEdit(Tag);
                    int contentId = Tag.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, Tag);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator."+ex.Message);
            }
            ViewBag.Categories = TagCategoryRepository.GetAll().OrderBy(r => r.Position).ToList().Select(r => new SelectListItem() { Text = r.Name.ToStr(), Value = r.Id.ToStr() }).ToList();
            return View(Tag);
        }



        //
        // GET: /Tag/Delete/5
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tag content = TagRepository.GetSingle(id);
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

            Tag Tag = TagRepository.GetSingle(id);
            if (Tag == null)
            {
                return HttpNotFound();
            }
            try
            {
                TagRepository.DeleteItem(Tag);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, Tag);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(Tag);

        }
    }
}