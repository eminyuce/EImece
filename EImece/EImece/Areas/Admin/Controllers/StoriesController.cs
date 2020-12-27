using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
        public ActionResult SaveOrEdit(Story story, int[] tags = null, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            try
            {
                if (story == null)
                {
                    return HttpNotFound();
                }

                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(postedImage,
                        story.ImageHeight,
                        story.ImageWidth,
                        EImeceImageType.StoryMainImage, story);

                    story.Lang = CurrentLanguage;
                    story = StoryService.SaveOrEditEntity(story);

                    if (tags != null)
                    {
                        StoryService.SaveStoryTags(story.Id, tags);
                    }

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
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, story);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.StackTrace);
            }
            ViewBag.Categories = StoryCategoryService.GetActiveBaseContents(null, 1);

            RemoveModelState();

            return View(story);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            Story story = StoryService.GetBaseContent(id);
            if (story == null)
            {
                return HttpNotFound();
            }
            try
            {
                StoryService.DeleteStoryById(id);
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, story);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        public ActionResult Media(int id)
        {
            return RedirectToAction("Index", "Media", new { contentId = id, mod = MediaModType.Stories, imageType = EImeceImageType.StoryGallery });
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
            var stories = StoryService.GetAdminPageList(0, search, CurrentLanguage);

            var result = from r in stories
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             StoryCategory = r.StoryCategory.ToStr(250),
                             Description = r.Description,
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                         };

            return DownloadFile(result, String.Format("Stories-{0}", GetCurrentLanguage));
        }
    }
}