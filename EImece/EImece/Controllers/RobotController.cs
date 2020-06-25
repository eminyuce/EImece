using EImece.Domain;
using EImece.Domain.Helpers.AttributeHelper;
using System;
using System.Text;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class RobotController : Controller
    {
        // GET: Robots
        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public FileContentResult RobotsText()
        {
            var content = "";
            String siteStatus = AppConfig.GetConfigString("SiteStatus", "dev");

            var builder = new UriBuilder(AppConfig.HttpProtocol, Request.Url.Host, Request.Url.Port);
            var fLink = String.Format("{1}{0}", "/sitemap.xml", builder.Uri);

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

            return File(Encoding.UTF8.GetBytes(content), "text/plain");
        }
    }
}