using Microsoft.Extensions.Logging;
using EImece.Web.Areas.Admin.Controllers;
using EImece.Domain.Entities;
using EImece.Web.Helpers;
using EImece.Domain.Helpers;
using EImece.Web.Filters;
using EImece.Domain.Models.Enums;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using EImece.Domain.Factories.IFactories;
using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class StoriesController : BaseAdminController
    {
        private const string IndexAction = "Index";

        protected IStoryService StoryService { get; }
        protected IStoryCategoryService StoryCategoryService { get; }
        protected IEntityFactory EntityFactory { get; }
        protected FilesHelper FilesHelper { get; }

        public StoriesController(ISettingService settingService,
            IStoryService storyService,
            IStoryCategoryService storyCategoryService,
            IEntityFactory entityFactory,
            FilesHelper filesHelper, ILogger<StoriesController> logger)
            : base(settingService, logger) {
            StoryService = storyService ?? throw new ArgumentNullException(nameof(storyService));
            StoryCategoryService = storyCategoryService ?? throw new ArgumentNullException(nameof(storyCategoryService));
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
            FilesHelper = filesHelper ?? throw new ArgumentNullException(nameof(filesHelper));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, int id = 0, String search = "")
        {
            int categoryId = id;

            var stories = await StoryService.GetAdminPageListAsync(categoryId, search, CurrentLanguage);
            return View(stories);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, int id = 0, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { id, search });
            }

            int categoryId = id;
            var stories = await StoryService.GetAdminPageListAsync(categoryId, search, CurrentLanguage);
            return AdminGridResult(stories);
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<Story>();
            var categories = await StoryCategoryService.GetActiveBaseContentsAsync(true, CurrentLanguage, cancellationToken);
            ViewBag.Categories = categories
                .OrderBy(c => c.Name.ToStr(), StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"), true))
                .ThenBy(c => c.Position)
                .ToList();

            if (id == 0)
            {
            }
            else
            {
                content = await StoryService.GetBaseContentAsync(id, cancellationToken);
            }

            return View(content);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(Story story, int[] tags = null, HttpPostedFileBase postedImage = null, String saveButton = null)
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
                    story = await StoryService.SaveOrEditEntityAsync(story);

                    if (tags != null)
                    {
                        await StoryService.SaveStoryTagsAsync(story.Id, tags);
                    }

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction(IndexAction);
                    }
                    else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    }
                }
                else
                {
                    ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to save changes:" + ex.StackTrace, story);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.StackTrace);
            }
            var cats = await StoryCategoryService.GetActiveBaseContentsAsync(null, CurrentLanguage);
            ViewBag.Categories = cats
                .OrderBy(c => c.Name.ToStr(), StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"), true))
                .ThenBy(c => c.Position)
                .ToList();

            RemoveModelState();

            return View(story);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            Story story = await StoryService.GetBaseContentAsync(id, cancellationToken);
            if (story == null)
            {
                return HttpNotFound();
            }
            try
            {
                await StoryService.DeleteStoryByIdAsync(id);
                SetSuccessMessage();
                return ReturnIndexIfNotUrlReferrer(IndexAction);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to delete product:" + ex.StackTrace, story);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer(IndexAction);
            }
        }

        [HttpGet]
        public ActionResult Media(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return RedirectToAction(IndexAction);
            }

            return RedirectToAction(IndexAction, "Media", new { contentId = id.Value, mod = MediaModType.Stories, imageType = EImeceImageType.StoryGallery });
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync(CancellationToken cancellationToken, string format = "excel")
        {
            return await DownloadFileAsync(format);
        }

        private async Task<ActionResult> DownloadFileAsync(string format = "excel")
        {
            String search = "";
            var stories = await StoryService.GetAdminPageListAsync(0, search, CurrentLanguage);

            var result = from r in stories
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             StoryCategory = r.StoryCategory.ToStr(250),
                             Description = r.Description,
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("Stories-{0}", GetCurrentLanguage), format);
        }
    }
}
