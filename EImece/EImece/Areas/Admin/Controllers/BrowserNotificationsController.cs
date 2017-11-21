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
    public class BrowserNotificationsController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            Expression<Func<BrowserNotification, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var result = BrowserNotificationService.SearchEntities(whereLambda, search,1);
            return View(result);
        }



        //
        // GET: /BrowserNotification/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var item = EntityFactory.GetBaseEntityInstance<BrowserNotification>();


            if (id == 0)
            {

            }
            else
            {
                item = BrowserNotificationService.GetSingle(id);
            }


            return View(item);
        }

        //
        // POST: /BrowserNotification/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(BrowserNotification BrowserNotification)
        {
            try
            {

                if (ModelState.IsValid)
                {


                    BrowserNotificationService.SaveOrEditEntity(BrowserNotification);
                    int itemId = BrowserNotification.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, BrowserNotification);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(BrowserNotification);
        }



        //
        [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BrowserNotification item = BrowserNotificationService.GetSingle(id);
            if (item == null)
            {
                return HttpNotFound();
            }


            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {

            BrowserNotification BrowserNotification = BrowserNotificationService.GetSingle(id);
            if (BrowserNotification == null)
            {
                return HttpNotFound();
            }
            try
            {
                BrowserNotificationService.DeleteEntity(BrowserNotification);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, BrowserNotification);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(BrowserNotification);

        }
    }
}
