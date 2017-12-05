using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
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
            var result = BrowserNotificationService.SearchEntities(whereLambda, search, 0);
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

                    if (BrowserNotification.IsSend)
                    {

                        var payLoad = new Dictionary<string, object>();
                        payLoad["body"] = BrowserNotification.Body;
                        payLoad["title"] = BrowserNotification.Name;
                        payLoad["imageurl"] = BrowserNotification.ImageUrl;
                        payLoad["redirectionurl"] = BrowserNotification.RedirectionUrl;
                        payLoad["notificationtype"] = ((NotificationType)BrowserNotification.NotificationType).ToString();

                        var subscribers = BrowserSubscriberService.GetBrowserSubscribers();
                        foreach (var subscriber in subscribers)
                        {
                            try
                            {
                                var item = new BrowserNotificationFeedBack();
                                item.Name = "Testing";
                                item.EntityHash = "";
                                item.IsActive = true;
                                item.Position = 1;
                                item.Lang = 1;
                                item.BrowserNotificationId = BrowserNotification.Id;
                                item.BrowserSubscriberId = subscriber.Id;
                                item.NotificationStatus = (int)NotificationStatus.NotTracked;
                                item.DateSend = DateTime.Now;
                                item.DateTracked = null;
                                BrowserNotificationFeedBackService.SaveOrEditEntity(item);
                                payLoad["BrowserNotificationFeedBackId"] = item.Id;
                                WebPushHelper.SendPushNotification(subscriber, payLoad);

                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                            }


                        }

                    }


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
        public ActionResult GetStats(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BrowserNotification item = BrowserNotificationService.GetStats(id);
            if (item == null)
            {
                return HttpNotFound();
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
