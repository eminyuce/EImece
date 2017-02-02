using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace EImece.Areas.Admin.Controllers
{
    public class StoriesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(int id = 0, String search = "")
        {
            int categoryId = id;
   
            var stories = StoryService.GetAdminPageList(categoryId, search, CurrentLanguage);
            return View(stories);
        }


        //
        // GET: /Story/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Story content = StoryService.GetBaseContent(id);
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

            var content = EntityFactory.GetBaseContentInstance<Story>();
            ViewBag.Categories = StoryCategoryService.GetActiveBaseContents(true, CurrentLanguage);

            if (id == 0)
            {

            }
            else
            {
                content = StoryService.GetBaseContent(id);
            }


            return View(content);
        }

        //
        // POST: /Story/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Story story, int[] tags = null, HttpPostedFileBase contentImage = null)
        {
            try
            {

                if (ModelState.IsValid)
                {


                    FilesHelper.SaveFileFromHttpPostedFileBase(contentImage,
                        story.ImageHeight,
                        story.ImageWidth,
                        EImeceImageType.StoryMainImage,story);
                

                    StoryService.SaveOrEditEntity(story);
                    int contentId = story.Id;

                    if (tags != null)
                    {
                        StoryService.SaveStoryTags(story.Id, tags);
                    }

                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, story);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator."+ex.StackTrace);
            }
            ViewBag.Categories = StoryCategoryService.GetActiveBaseContents(null, 1);
            return View(story);
        }



        //
        [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Story content = StoryService.GetBaseContent(id);
            if (content == null)
            {
                return HttpNotFound();
            }


            return View(content);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {

            Story Story = StoryService.GetBaseContent(id);
            if (Story == null)
            {
                return HttpNotFound();
            }
            try
            {
                StoryService.DeleteStoryById(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, Story);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(Story);

        }
        public ActionResult Media(int id)
        {
            return RedirectToAction("Index", "Media", new { contentId = id, mod = MediaModType.Stories, imageType = EImeceImageType.StoryGallery });
        }
    }
}