using EImece.Domain;
using EImece.Domain.Helpers.AttributeHelper;
using NLog; // Added for logging
using System;
using System.Text;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class RobotController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // Added logger instance
        private const string TextPlain = "text/plain";

        // GET: Robots
        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public FileContentResult RobotsText()
        {
            Logger.Info("Entering RobotsText action.");

            var content = "";
            Logger.Info($"Checking site status: IsSiteUnderConstruction = {AppConfig.IsSiteUnderConstruction}, IsSiteUnderDevelopment = {AppConfig.IsSiteUnderDevelopment}, IsSiteLive = {AppConfig.IsSiteLive}");

            if (AppConfig.IsSiteUnderConstruction || AppConfig.IsSiteUnderDevelopment)
            {
                Logger.Info("Site is under construction or development. Setting robots.txt to disallow all.");
                content = "Disallow: /" + Environment.NewLine;
                content += "# Disallow Robots (Debug)" + Environment.NewLine;
            }
            else if (AppConfig.IsSiteLive)
            {
                Logger.Info("Site is live. Configuring robots.txt with sitemap and specific disallows.");
                var builder = new UriBuilder(AppConfig.HttpProtocol, Request.Url.Host, Request.Url.Port);
                builder.Path += "sitemap.xml";
                var fLink = builder.Uri;
                Logger.Info($"Generated sitemap URL: {fLink}");

                content += "User-agent:* " + Environment.NewLine;
                content += "Sitemap: " + fLink + Environment.NewLine;
                content += "Disallow: /Ajax/ " + Environment.NewLine;
                content += "Disallow: /Error/ " + Environment.NewLine;
                content += "Disallow: /Manage/ " + Environment.NewLine;
                content += "Disallow: /Account/ " + Environment.NewLine;
                content += "Disallow: /Admin/ " + Environment.NewLine;
                content += "Disallow: /Customer/ " + Environment.NewLine;
                content += "# Allow Robots (Release)" + Environment.NewLine;
            }
            else
            {
                Logger.Info("No specific site status matched. Returning empty robots.txt content.");
            }

            return File(Encoding.UTF8.GetBytes(content), TextPlain);
        }
    }
}