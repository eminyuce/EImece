using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class TagsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index(String search = "")
        {
            var result = TagService.GetAdminPageList(search, CurrentLanguage);
            return View(result);
        }

        private List<SelectListItem> GetCategoriesSelectList()
        {
            List<TagCategory> tagCategories = TagCategoryService.GetAll().Where(r => r.IsActive).OrderBy(r => r.Position).ToList();
            return tagCategories.Select(r => new SelectListItem()
            {
                Text = r.Name.ToStr(),
                Value = r.Id.ToStr()
            }).ToList();
        }

        //
        // GET: /Tag/Create

        public ActionResult SaveOrEdit(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Tag>();
            ViewBag.Categories = GetCategoriesSelectList();
            TempData[Constants.TempDataReturnUrlReferrer] = Request.UrlReferrer.ToStr();
            if (id == 0)
            {
            }
            else
            {
                content = TagService.GetSingle(id);
            }

            return View(content);
        }

        //
        // POST: /Tag/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Tag tag)
        {
            try
            {
                if (tag == null)
                {
                    return HttpNotFound();
                }

                if (ModelState.IsValid)
                {
                    tag.Lang = CurrentLanguage;
                    TagService.SaveOrEditEntity(tag);
                    return RedirectToAction("Index");

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, tag);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            ViewBag.Categories = GetCategoriesSelectList();
            return View(tag);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            Tag tag = TagService.GetSingle(id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            try
            {
                TagService.DeleteEntity(tag);
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, tag);
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
            var tags = TagService.GetAdminPageList(search, CurrentLanguage);

            var result = from r in tags
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             TagCategory = r.TagCategory.Name.ToStr(250),
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                         };

            return DownloadFile(result, String.Format("Tags-{0}", GetCurrentLanguage));
        }
    }
}