using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class StoryCategoriesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index(String search = "")
        {
            Expression<Func<StoryCategory, bool>> whereLambda = r => r.Name.Contains(search);
            var categories = StoryCategoryService.SearchEntities(whereLambda, search, CurrentLanguage);
            return View(categories);
        }

        //
        // GET: /StoryCategory/Create

        public ActionResult SaveOrEdit(int id = 0)
        {
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
        public ActionResult SaveOrEdit(StoryCategory storyCategory, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            try
            {
                if (storyCategory == null)
                {
                    return HttpNotFound();
                }
                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(postedImage,
                       storyCategory.ImageHeight,
                       storyCategory.ImageWidth,
                       EImeceImageType.StoryCategoryMainImage,
                       storyCategory);
                    storyCategory.Lang = CurrentLanguage;
                    StoryCategoryService.SaveOrEditEntity(storyCategory);
                    int contentId = storyCategory.Id;

                    MenuService.UpdateStoryCategoryMenuLink(contentId, CurrentLanguage);

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }
                    else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, storyCategory);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            RemoveModelState();
            return View(storyCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StoryCategory StoryCategory = StoryCategoryService.GetSingle(id);
            if (StoryCategory == null)
            {
                return HttpNotFound();
            }
            try
            {
                StoryCategoryService.DeleteStoryCategoryById(id);
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, StoryCategory);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync()
        {
            return await Task.Run(() =>
            {
                return DownloadFile();
            }).ConfigureAwait(true);
        }

        private ActionResult DownloadFile()
        {
            String search = "";
            Expression<Func<StoryCategory, bool>> whereLambda = r => r.Name.Contains(search);
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