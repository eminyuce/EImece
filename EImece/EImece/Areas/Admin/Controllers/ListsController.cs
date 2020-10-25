using EImece.Domain.Entities;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using NLog;
using Resources;
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
            Expression<Func<List, bool>> whereLambda = r => r.Name.Contains(search);
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
        public ActionResult SaveOrEdit(List list, string itemText)
        {
            if (list == null)
            {
                throw new ArgumentException("list cannot be empty");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    list.Lang = CurrentLanguage;
                    list = ListService.SaveOrEditEntity(list);
                    var listItems = list.SetListItems(itemText);
                    ListItemService.SaveListItem(list.Id, listItems);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, list);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            return View(list);
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
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return View(List);
        }
    }
}