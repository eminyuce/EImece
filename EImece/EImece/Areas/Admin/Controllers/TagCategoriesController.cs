using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Filters;
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
using System.Web.Mvc;

using EImece.Domain.Factories.IFactories;
using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class TagCategoriesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected ITagCategoryService TagCategoryService { get; }
        protected IEntityFactory EntityFactory { get; }

        public TagCategoriesController(
            ISettingService settingService,
            ITagCategoryService tagCategoryService,
            IEntityFactory entityFactory)
            : base(settingService)
        {
            TagCategoryService = tagCategoryService ?? throw new ArgumentNullException(nameof(tagCategoryService));
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<TagCategory, bool>> whereLambda = r => r.Name.Contains(search);
            var tags = await TagCategoryService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(tags);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            Expression<Func<TagCategory, bool>> whereLambda = r => r.Name.Contains(search);
            var tags = await TagCategoryService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return new QueryableResult<TagCategory>(tags.AsQueryable());
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<TagCategory>();

            if (id == 0)
            {
            }
            else
            {
                content = await TagCategoryService.GetSingleAsync(id);
            }

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(TagCategory TagCategory, String saveButton = null)
        {
            if (TagCategory == null)
            {
                throw new ArgumentException("TagCategory cannot be empty");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    TagCategory.Lang = CurrentLanguage;
                    await TagCategoryService.SaveOrEditEntityAsync(TagCategory);

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                }
                else
                {
                    ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, TagCategory);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            RemoveModelState();
            return View(TagCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            TagCategory tagCategory = await TagCategoryService.GetSingleAsync(id);
            if (tagCategory == null)
            {
                return HttpNotFound();
            }
            try
            {
                await TagCategoryService.DeleteTagCategoryByIdAsync(id);
                SetSuccessMessage();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, tagCategory);
                SetErrorMessage();
                return RedirectToAction("Index");
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
            Expression<Func<TagCategory, bool>> whereLambda = r => r.Name.Contains(search);
            var tags = await TagCategoryService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);

            var result = from r in tags
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("TagCategories-{0}", GetCurrentLanguage), format);
        }
    }
}
