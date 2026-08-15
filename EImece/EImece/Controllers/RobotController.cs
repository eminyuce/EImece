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

        // GET: Robots
        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public FileContentResult RobotsText()
        {
            Logger.Info("Entering RobotsText action.");

            var sb = new StringBuilder(512);

            if (!SeoSettings.AllowIndexing)
            {
                Logger.Info("Search engine indexing is disabled. Setting robots.txt to disallow all.");
                sb.AppendLine(Constants.RobotsUserAgentAll)
                  .AppendLine("Disallow: /");
            }
            else if (AppConfig.IsSiteUnderConstruction || AppConfig.IsSiteUnderDevelopment)
            {
                Logger.Info("Site is under construction or development. Setting robots.txt to disallow all.");
                sb.AppendLine(Constants.RobotsUserAgentAll)
                  .AppendLine("Disallow: /")
                  .AppendLine("# Disallow Robots (Debug)");
            }
            else if (AppConfig.IsSiteLive)
            {
                string host = !string.IsNullOrWhiteSpace(AppConfig.Domain) ? AppConfig.Domain : (Request?.Url != null ? Request.Url.Authority : "localhost");
                string protocol = !string.IsNullOrWhiteSpace(AppConfig.HttpProtocol) ? AppConfig.HttpProtocol : "https";
                string sitemapUrl = $"{protocol}://{host}/sitemap.xml";
                Logger.Info($"Generated sitemap URL: {sitemapUrl}");

                sb.AppendLine(Constants.RobotsUserAgentAll)
                  .AppendLine("Allow: /")
                  .Append("Sitemap: ").AppendLine(sitemapUrl)
                  .AppendLine("Disallow: /Ajax/ ")
                  .AppendLine("Disallow: /Error/ ")
                  .AppendLine("Disallow: /Manage/ ")
                  .AppendLine("Disallow: /Account/ ")
                  .AppendLine("Disallow: /Admin/ ")
                  .AppendLine("Disallow: /Customer/ ")
                  .AppendLine("# Allow Robots (Release)");
            }
            else
            {
                Logger.Info("No specific site status matched. Returning allow-all robots.txt.");
                sb.AppendLine(Constants.RobotsUserAgentAll)
                  .AppendLine("Allow: /");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), TextPlain);
        }
    }
}
