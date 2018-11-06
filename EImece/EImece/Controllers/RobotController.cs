using EImece.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Helpers.AttributeHelper;

namespace EImece.Controllers
{
    public class RobotController : Controller
    {
        // GET: Robots
        [CustomOutputCache(CacheProfile = "Cache30Days")]
        public FileContentResult RobotsText()
        {
            var content = "";
            //var builder = new UriBuilder(Request.Url.Scheme, Settings.HttpProtocol, Request.Url.Port);
            //var fLink = String.Format("{1}{0}", "/news/sitemap.xml", builder.Uri.ToString().TrimEnd('/'));
            //content += "Sitemap: " + fLink + Environment.NewLine;
            //fLink = String.Format("{1}{0}", "/sitemap.xml", builder.Uri.ToString().TrimEnd('/'));
            //content += "Sitemap: " + fLink + Environment.NewLine;
            //content += "User-agent: *" + Environment.NewLine;


            String siteStatus = ApplicationConfigs.GetConfigString("SiteStatus", "dev");

            var builder = new UriBuilder(ApplicationConfigs.HttpProtocol, Request.Url.Host);
            var fLink = String.Format("{1}{0}", "/sitemap.xml", builder.Uri.ToString().TrimEnd('/'));
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