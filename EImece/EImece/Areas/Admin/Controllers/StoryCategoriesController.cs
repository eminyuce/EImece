using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using System;
using System.Data;
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
            var categories = StoryCategoryService.SearchEntities(whereLambda, search, CurrentLanguage);
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

            StoryCategory content = StoryCategoryService.GetBaseContent(id);
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
            TempData[TempDataReturnUrlReferrer] = Request.UrlReferrer.ToStr();
            var content = EntityFactory.GetBaseEntityInstance<StoryCategory>();


            if (id == 0)
            {

            }
            else
            {
                content = StoryCategoryService.GetBaseContent(id);
            }


            return View(content);
        }

        //
        // POST: /StoryCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(StoryCategory storyCategory, HttpPostedFileBase contentImage = null)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    FilesHelper.SaveFileFromHttpPostedFileBase(contentImage,
                       storyCategory.ImageHeight,
                       storyCategory.ImageWidth,
                       EImeceImageType.StoryCategoryMainImage,
                       storyCategory);
                    storyCategory.Lang = CurrentLanguage;
                    StoryCategoryService.SaveOrEditEntity(storyCategory);
                    int contentId = storyCategory.Id;

                    MenuService.UpdateStoryCategoryMenuLink(contentId, CurrentLanguage);
                    return ReturnTempUrl("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, storyCategory);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator." + ex.Message);
            }

            return View(storyCategory);
        }



        //
        [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StoryCategory content = StoryCategoryService.GetBaseContent(id);
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
                StoryCategoryService.DeleteStoryCategoryById(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, StoryCategory);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(StoryCategory);

        }

        public ActionResult ExportExcel()
        {
            String search = "";
            Expression<Func<StoryCategory, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var categories = StoryCategoryService.SearchEntities(whereLambda, search, CurrentLanguage);

            var result = from r in categories
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             Description = r.Description,
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                         };

            return DownloadFile(result, String.Format("StoryCategories-{0}", GetCurrentLanguage));


        }

    }
}