﻿using EImece.Domain.Entities;
using EImece.Domain.Helpers;
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
        public ActionResult Index(String search = "")
        {
            Expression<Func<Story, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var stories = StoryService.SearchEntities(whereLambda, search);
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

            var content = new Story();
            ViewBag.Categories = StoryCategoryService.GetActiveBaseContents(null,1);

            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
            }
            else
            {
                content = StoryService.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
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
                        var mainImage = ImageHelper.SaveFileFromHttpPostedFileBase(contentImage, 0, 0, EImeceImageType.StoryMainImage);
                        FileStorageRepository.SaveOrEdit(mainImage);
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
        // GET: /Story/Delete/5
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
    }
}