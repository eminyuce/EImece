using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.AttributeHelper;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class TagCategoriesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            Expression<Func<TagCategory, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
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
            try
            {

                if (ModelState.IsValid)
                {

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
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(TagCategory);
        }



        //
            [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TagCategory content = TagCategoryService.GetSingle(id);
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

            TagCategory TagCategory = TagCategoryService.GetSingle(id);
            if (TagCategory == null)
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
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, TagCategory);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(TagCategory);

        }
    }
}