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
        public ActionResult Index(int id=0,String search = "")
        {
            int categoryId = id;
            int lang = Settings.MainLanguage;
            var stories = StoryService.GetAdminPageList(categoryId, search, lang);
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

            Story content = StoryService.GetSingle(id);
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

            var content = Story.GetInstance<Story>();
            ViewBag.Categories = StoryCategoryService.GetActiveBaseContents(null,1);

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
        public ActionResult SaveOrEdit(Story Story, HttpPostedFileBase contentImage = null)
        {
            try
            {

                if (ModelState.IsValid)
                {


                    if (contentImage != null)
                    {
                        var mainImage = FilesHelper.SaveFileFromHttpPostedFileBase(contentImage, 0, 0, EImeceImageType.StoryMainImage);
                        FileStorageService.SaveOrEditEntity(mainImage);
                        Story.MainImageId = mainImage.Id;
                    }

                    StoryService.SaveOrEditEntity(Story);
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
            ViewBag.Categories = StoryCategoryService.GetActiveBaseContents(null, 1);
            return View(Story);
        }



        //
        [AuthorizeRoles(Settings.AdministratorRole)]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Story content = StoryService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }


            return View(content);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(Settings.AdministratorRole)]
        public ActionResult DeleteConfirmed(int id)
        {

            Story Story = StoryService.GetSingle(id);
            if (Story == null)
            {
                return HttpNotFound();
            }
            try
            {
                StoryService.DeleteEntity(Story);
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