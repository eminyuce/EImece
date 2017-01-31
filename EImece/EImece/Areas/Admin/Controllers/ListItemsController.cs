using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
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
    public class ListItemsController : BaseAdminController
    {
        // GET: Admin/ListItem
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            Expression<Func<ListItem, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var tags = ListItemService.SearchEntities(whereLambda, search);
            return View(tags);
        }


        private List<SelectListItem> GetListsDropDown()
        {
            var lists = ListService.GetActiveBaseEntities(true, Settings.MainLanguage);

            var resultListItem = new List<SelectListItem>();
            foreach (var item in lists)
            {
                resultListItem.Add(new SelectListItem() { Text = item.Name, Value = item.Id.ToStr() });
            }
            return resultListItem;
        }
        //
        // GET: /ListItem/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = EntityFactory.GetBaseEntityInstance<ListItem>();
            ViewBag.Lists = GetListsDropDown();

            if (id == 0)
            {

            }
            else
            {
                content = ListItemService.GetSingle(id);
            }


            return View(content);
        }

        //
        // POST: /ListItem/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(ListItem ListItem)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    ListItemService.SaveOrEditEntity(ListItem);
                    int contentId = ListItem.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, ListItem);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(ListItem);
        }



        //
        [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ListItem content = ListItemService.GetSingle(id);
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

            ListItem ListItem = ListItemService.GetSingle(id);
            if (ListItem == null)
            {
                return HttpNotFound();
            }
            try
            {
                ListItemService.DeleteEntity(ListItem);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, ListItem);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(ListItem);

        }
    }
}