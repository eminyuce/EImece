using EImece.Domain;
using EImece.Domain.Helpers.AttributeHelper;
using System;
using System.Text;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class RobotController : Controller
    {
        private const string TextPlain = "text/plain";

        // GET: Robots
        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public FileContentResult RobotsText()
        {
            try
            {
                var content = "";
                if (AppConfig.IsSiteUnderConstruction)
                {
                    return File(Encoding.UTF8.GetBytes(content), TextPlain);
                }
                String siteStatus = AppConfig.GetConfigString("SiteStatus", "dev");

                var builder = new UriBuilder(AppConfig.HttpProtocol, Request.Url.Host, Request.Url.Port);
                builder.Path += "sitemap.xml";
                var fLink = builder.Uri;
                content += "User-agent:*: " + Environment.NewLine;
                content += "Sitemap: " + fLink + Environment.NewLine;

                content += "Disallow: /Ajax/ " + Environment.NewLine;
                content += "Disallow: /Error/ " + Environment.NewLine;
                content += "Disallow: /Manage/ " + Environment.NewLine;
                content += "Disallow: /Account/ " + Environment.NewLine;
                if (string.Equals(siteStatus, "live", StringComparison.InvariantCultureIgnoreCase))
                {
                    content += "# Allow Robots (Release)" + Environment.NewLine;
                }
                else
                {
                    content += "Disallow: /" + Environment.NewLine;
                    content += "# Disallow Robots (Debug)" + Environment.NewLine;
                }
                byte [] result = Encoding.UTF8.GetBytes(content);
                return File(result, TextPlain);
            }
            catch (Exception ex)
            {
                return EmptyContent("");
            }
      
        }
    }
}