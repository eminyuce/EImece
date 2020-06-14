using EImece.Domain.Entities;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebPush;

namespace EImece.Domain.Helpers
{
    public class WebPushHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static String SendPushNotification(BrowserSubscriber p, Dictionary<string, object> payLoad)
        {
            var subscription = new PushSubscription(p.EndPoint, p.P256dh, p.Auth);

            var options = new Dictionary<string, object>();
            //vapidDetails should be a VapidDetails object with subject, publicKey and privateKey values defined. These values should follow the VAPID Spec.
            //
            options["vapidDetails"] = new VapidDetails(
                p.BrowserSubscription.Subject,
                p.BrowserSubscription.PublicKey,
                p.BrowserSubscription.PrivateKey);

            var payLoadJson = JsonConvert.SerializeObject(payLoad);

            var webPushClient = new WebPushClient();
            try
            {
                var message = webPushClient.GenerateRequestDetails(subscription, @"test payload", options);
                var authorizationHeader = message.Headers.GetValues(@"Authorization").First();
                Dictionary<string, string> ss = message.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value));
                Logger.Info("message.Headers = " + GetLine(ss));

                webPushClient.SendNotificationAsync(subscription, payLoadJson, options);
                Logger.Info("SendNotification IS SENT ");
                return "SUCCESS";
            }
            catch (WebPushException exception)
            {
                return String.Format("{0} {1} {2}", exception.StatusCode, exception.Message, exception.StackTrace);
            }
        }

        public static string GetLine(Dictionary<string, string> d)
        {
            // Build up each line one-by-one and then trim the end
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in d)
            {
                builder.Append(pair.Key).Append(":").Append(pair.Value).Append(',');
                builder.AppendLine("</br>");
            }
            string result = builder.ToString();
            // Remove the final delimiter
            result = result.TrimEnd(',');
            return result;
        }
    }
}