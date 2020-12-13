using EImece.Domain;
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
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, tagCategory);
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
            Expression<Func<TagCategory, bool>> whereLambda = r => r.Name.Contains(search);
            var tags = TagCategoryService.SearchEntities(whereLambda, search, CurrentLanguage);

            var result = from r in tags
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                         };

            return DownloadFile(result, String.Format("TagCategories-{0}", GetCurrentLanguage));
        }
    }
}