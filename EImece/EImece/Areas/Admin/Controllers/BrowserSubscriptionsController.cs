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
    public class BrowserSubscriptionsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
     
        public ActionResult Index(String search = "")
        {
            var result = BrowserSubscriptionService.GetActiveBaseEntities(null, CurrentLanguage);
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

            BrowserSubscription content = BrowserSubscriptionService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }
    
        //
        // GET: /Tag/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = EntityFactory.GetBaseEntityInstance<BrowserSubscription>();
  
            TempData[TempDataReturnUrlReferrer] = Request.UrlReferrer.ToStr();
            if (id == 0)
            {

            }
            else
            {
                content = BrowserSubscriptionService.GetSingle(id);
            }


            return View(content);
        }

        //
        // POST: /Tag/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(BrowserSubscription item)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    item.Lang = CurrentLanguage;
                    BrowserSubscriptionService.SaveOrEditEntity(item);
                    int contentId = item.Id;
                    return ReturnTempUrl("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, item);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator." + ex.Message);
            }
            
            return View(item);
        }



        //
        [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BrowserSubscription content = BrowserSubscriptionService.GetSingle(id);
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

            BrowserSubscription item = BrowserSubscriptionService.GetSingle(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            try
            {
                BrowserSubscriptionService.DeleteEntity(item);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, item);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(item);

        }
    }
}