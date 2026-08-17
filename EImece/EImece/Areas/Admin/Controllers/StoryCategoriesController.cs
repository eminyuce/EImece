using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class StoryCategoriesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<StoryCategory, bool>> whereLambda = r => r.Name.Contains(search);
            var categories = await StoryCategoryService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(categories);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            Expression<Func<StoryCategory, bool>> whereLambda = r => r.Name.Contains(search);
            var categories = await StoryCategoryService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return new QueryableResult<StoryCategory>(categories.AsQueryable());
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<StoryCategory>();

            if (id == 0)
            {
            }
            else
            {
                content = await StoryCategoryService.GetBaseContentAsync(id, cancellationToken);
            }

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(StoryCategory storyCategory, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            try
            {
                if (storyCategory == null)
                {
                    return HttpNotFound();
                }

                if (storyCategory != null && string.IsNullOrEmpty(storyCategory.PageTheme))
                {
                    ModelState.AddModelError("PageTheme", AdminResource.PageThemeSelectRequired);
                    return View(storyCategory);
                }

                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(postedImage,
                       storyCategory.ImageHeight,
                       storyCategory.ImageWidth,
                       EImeceImageType.StoryCategoryMainImage,
                       storyCategory);
                    storyCategory.Lang = CurrentLanguage;
                    await StoryCategoryService.SaveOrEditEntityAsync(storyCategory);
                    int contentId = storyCategory.Id;

                    await MenuService.UpdateStoryCategoryMenuLinkAsync(contentId, CurrentLanguage);

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
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
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, storyCategory);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            RemoveModelState();
            return View(storyCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StoryCategory StoryCategory = await StoryCategoryService.GetSingleAsync(id);
            if (StoryCategory == null)
            {
                return HttpNotFound();
            }
            try
            {
                await StoryCategoryService.DeleteStoryCategoryByIdAsync(id);
                SetSuccessMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, StoryCategory);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync(CancellationToken cancellationToken, string format = "excel")
        {
            return await DownloadFileAsync(format);
        }

        private async Task<ActionResult> DownloadFileAsync(string format = "excel")
        {
            String search = "";
            Expression<Func<StoryCategory, bool>> whereLambda = r => r.Name.Contains(search);
            var categories = await StoryCategoryService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);

            var result = from r in categories
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             Description = r.Description,
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("StoryCategories-{0}", GetCurrentLanguage), format);
        }
    }
}
