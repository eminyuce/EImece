using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
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


        //
        // GET: /Tag/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tag content = TagService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }
        private List<SelectListItem> GetCategoriesSelectList()
        {

            List<TagCategory> tagCategories = TagCategoryService.GetAll().OrderBy(r => r.Position).ToList();
            return tagCategories.Select(r => new SelectListItem()
            {
                Text =

                String.Format("{0}", r.Name.ToStr())
                ,
                Value = r.Id.ToStr()
            }).ToList();
        }
        //
        // GET: /Tag/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = EntityFactory.GetBaseEntityInstance<Tag>();
            ViewBag.Categories = GetCategoriesSelectList();
            TempData[TempDataReturnUrlReferrer] = Request.UrlReferrer.ToStr();
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
        public ActionResult SaveOrEdit(Tag Tag)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    Tag.Lang = CurrentLanguage;
                    TagService.SaveOrEditEntity(Tag);
                    int contentId = Tag.Id;
                    return ReturnTempUrl("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, Tag);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator." + ex.Message);
            }
            ViewBag.Categories = GetCategoriesSelectList();
            return View(Tag);
        }



        //
        [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tag content = TagService.GetSingle(id);
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

            Tag Tag = TagService.GetSingle(id);
            if (Tag == null)
            {
                return HttpNotFound();
            }
            try
            {
                TagService.DeleteEntity(Tag);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, Tag);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(Tag);

        }

        public ActionResult ExportExcel()
        {
            String search = "";
            var tags = TagService.GetAdminPageList(search, CurrentLanguage);
            DataTable dt = new DataTable();
            dt.TableName = "Tags";

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
            dt = GeneralHelper.LINQToDataTable(result);

            var ms = ExcelHelper.GetExcelByteArrayFromDataTable(dt);
            return File(ms, "application/vnd.ms-excel",
                String.Format("Tags-{0}-{1}.xls", GetCurrentLanguage,
                DateTime.Now.ToString("yyyy-MM-dd")));


        }
    }
}