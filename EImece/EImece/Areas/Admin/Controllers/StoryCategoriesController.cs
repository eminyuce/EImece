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
    public class StoryCategoriesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            Expression<Func<StoryCategory, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var categories = StoryCategoryService.SearchEntities(whereLambda, search);
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

            StoryCategory content = StoryCategoryService.GetSingle(id);
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

            var content = StoryCategory.GetInstance<StoryCategory>();  
           

            if (id == 0)
            {
               
            }
            else
            {
                content = StoryCategoryService.GetSingle(id);
            }


            return View(content);
        }

        //
        // POST: /StoryCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(StoryCategory StoryCategory, HttpPostedFileBase contentImage = null)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    if (contentImage != null)
                    {
                        var mainImage = FilesHelper.SaveFileFromHttpPostedFileBase(contentImage, 0, 0, EImeceImageType.StoryCategoryMainImage);
                        FileStorageService.SaveOrEditEntity(mainImage);
                        StoryCategory.MainImageId = mainImage.Id;
                    }

                    StoryCategoryService.SaveOrEditEntity(StoryCategory);
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
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator."+ex.Message);
            }
 
            return View(StoryCategory);
        }



        //
            [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StoryCategory content = StoryCategoryService.GetSingle(id);
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

            StoryCategory StoryCategory = StoryCategoryService.GetSingle(id);
            if (StoryCategory == null)
            {
                return HttpNotFound();
            }
            try
            {
                StoryCategoryService.DeleteEntity(StoryCategory);
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