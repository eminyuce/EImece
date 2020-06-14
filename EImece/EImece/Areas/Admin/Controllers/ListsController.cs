using EImece.Domain.Entities;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using NLog;
using System;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ListsController : BaseAdminController
    {
        // GET: Admin/List
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index(String search = "")
        {
            Expression<Func<List, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var tags = ListService.SearchEntities(whereLambda, search, CurrentLanguage);
            return View(tags);
        }

        //
        // GET: /List/Create
        /****
         *
         *
         *
         *cat, 1
        bird, 2
        dog, 3
        fish, 4
        parrot, 5
        hamster, 6
        dove, 7
        */

        public ActionResult SaveOrEdit(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<List>();

            if (id == 0)
            {
            }
            else
            {
                content = ListService.GetListById(id);
            }

            return View(content);
        }

        //
        // POST: /List/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(List List, string itemText)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List.Lang = CurrentLanguage;
                    ListService.SaveOrEditEntity(List);
                    ListItemService.SaveListItem(List.Id, List.SetListItems(itemText));

                    int contentId = List.Id;
                    return RedirectToAction("Index");
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, List);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator." + ex.Message);
            }

            return View(List);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            List List = ListService.GetSingle(id);
            if (List == null)
            {
                return HttpNotFound();
            }
            try
            {
                ListService.DeleteListById(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, List);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(List);
        }
    }
}