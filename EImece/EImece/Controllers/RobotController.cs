using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using NLog;
using System;
using System.Text;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class RobotController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string TextPlain = "text/plain";
        private const string UserAgentAll = "User-agent: *";

        // GET: Robots
        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public FileContentResult RobotsText()
        {
            Logger.Info("Entering RobotsText action.");

            string content;

            if (!SeoSettings.AllowIndexing)
            {
                Logger.Info("Search engine indexing is disabled. Setting robots.txt to disallow all.");
                content = UserAgentAll + Environment.NewLine
                        + "Disallow: /" + Environment.NewLine;
            }
            else if (AppConfig.IsSiteUnderConstruction || AppConfig.IsSiteUnderDevelopment)
            {
                Logger.Info("Site is under construction or development. Setting robots.txt to disallow all.");
                content = UserAgentAll + Environment.NewLine
                        + "Disallow: /" + Environment.NewLine
                        + "# Disallow Robots (Debug)" + Environment.NewLine;
            }
            else if (AppConfig.IsSiteLive)
            {
                Logger.Info("Site is live. Configuring robots.txt with sitemap and specific disallows.");
                var builder = new UriBuilder(AppConfig.HttpProtocol, Request.Url.Host, Request.Url.Port);
                builder.Path += "sitemap.xml";
                var fLink = builder.Uri;
                Logger.Info($"Generated sitemap URL: {fLink}");

                content = UserAgentAll + Environment.NewLine
                        + "Allow: /" + Environment.NewLine
                        + "Sitemap: " + fLink + Environment.NewLine
                        + "Disallow: /Ajax/ " + Environment.NewLine
                        + "Disallow: /Error/ " + Environment.NewLine
                        + "Disallow: /Manage/ " + Environment.NewLine
                        + "Disallow: /Account/ " + Environment.NewLine
                        + "Disallow: /Admin/ " + Environment.NewLine
                        + "Disallow: /Customer/ " + Environment.NewLine
                        + "# Allow Robots (Release)" + Environment.NewLine;
            }
            else
            {
                Logger.Info("No specific site status matched. Returning allow-all robots.txt.");
                content = UserAgentAll + Environment.NewLine
                        + "Allow: /" + Environment.NewLine;
            }

            return File(Encoding.UTF8.GetBytes(content), TextPlain);
        }
    }
}
