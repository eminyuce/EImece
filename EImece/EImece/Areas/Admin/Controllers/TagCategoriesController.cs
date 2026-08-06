using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class TagCategoriesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index(String search = "")
        {
            Expression<Func<TagCategory, bool>> whereLambda = r => r.Name.Contains(search);
            var tags = TagCategoryService.SearchEntities(whereLambda, search, CurrentLanguage);
            return View(tags);
        }

        //
        // GET: /TagCategory/Create

        public ActionResult SaveOrEdit(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<TagCategory>();

            if (id == 0)
            {
            }
            else
            {
                content = TagCategoryService.GetSingle(id);
            }

            return View(content);
        }

        //
        // POST: /TagCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(TagCategory TagCategory)
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
                    TagCategoryService.SaveOrEditEntity(TagCategory);
                    int contentId = TagCategory.Id;
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, TagCategory);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return View(TagCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            TagCategory tagCategory = TagCategoryService.GetSingle(id);
            if (tagCategory == null)
            {
                return HttpNotFound();
            }
            try
            {
                TagCategoryService.DeleteTagCategoryById(id);
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
        public async Task<ActionResult> ExportExcelAsync(string format = "excel")
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