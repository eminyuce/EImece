using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class AjaxController : BaseController
    {
        // GET: Ajax
        public ActionResult Index()
        {
            return View();
        }
        // GET: Ajax{ "endpoint": endpoint, "p256dh": p256dh, "auth": auth, 
        public ActionResult GetSubscribersData(String endpoint,
            String p256dh,
            String auth,
            String applicationServerPublicKey,
            String userAgent)
        {
            BrowserSubscription SubscriptionList = BrowserSubscriptionService.GetSubscriptionsByPublicApiKey(applicationServerPublicKey);
            if (SubscriptionList != null)
            {

                var item = new BrowserSubscriber();
                item.BrowserSubscriptionId = SubscriptionList.Id;
                item.EndPoint = endpoint;
                item.Auth = auth;
                item.P256dh = p256dh;
                item.UserAgent = userAgent;
                item.UserAddress = GeneralHelper.GetRemoteIP(Request.ServerVariables);
                item.IsActive = true;
                item.Name = "Testing";
                item.EntityHash = "EntityHash";
                item.Position = 1;
                item.Lang = 1;
                BrowserSubscriberService.SaveOrEditEntity(item);
                return Json("true", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("false", JsonRequestBehavior.AllowGet);
            }


        }

        [CompressContent]
        [CustomOutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult ServiceWorkerScript()
        {
            var keys = BrowserSubscriptionService.GetAll();
            Response.ContentType = "text/javascript";

            int swVersion = 1;
            String chromePublicKey = "";
            String firefoxPublicKey = "";
            String safariPublicKey = "";
            if (keys.Any(i => i.BrowserType == (int)BrowserType.Chrome))
            {
                BrowserSubscription chromeItem = keys.FirstOrDefault(i => i.BrowserType == (int)BrowserType.Chrome);
                chromePublicKey = (chromeItem != null ? chromeItem.PublicKey : "");
            }
            if (keys.Any(i => i.BrowserType == (int)BrowserType.Firefox))
            {
                BrowserSubscription firefoxItem = keys.FirstOrDefault(i => i.BrowserType == (int)BrowserType.Firefox);
                firefoxPublicKey = (firefoxItem != null ? firefoxItem.PublicKey : "");
            }
            if (keys.Any(i => i.BrowserType == (int)BrowserType.Safari))
            {
                BrowserSubscription safariItem = keys.FirstOrDefault(i => i.BrowserType == (int)BrowserType.Safari);
                safariPublicKey = (safariItem != null ? safariItem.PublicKey : "");
            }
            string jsFileText = FileManagerHelper.GetServiceWorkerScript();
            jsFileText = jsFileText.Replace("[ChromePublicKey]", chromePublicKey);
            jsFileText = jsFileText.Replace("[FirefoxPublickKey]", firefoxPublicKey);
            jsFileText = jsFileText.Replace("[SafariPublicKey]", safariPublicKey);
            jsFileText = jsFileText.Replace("[ApplicationServerUrl]", Settings.Domain);
            jsFileText = jsFileText.Replace("[LocalSwVersion]", swVersion+"");

            return Content(jsFileText);
        }
    }
}