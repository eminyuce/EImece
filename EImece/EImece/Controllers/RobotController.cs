using EImece.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class RobotController : Controller
    {
        // GET: Robots
        [OutputCache(CacheProfile = "Cache30Days")]
        public FileContentResult RobotsText()
        {
            var content = "";
            //var builder = new UriBuilder(Request.Url.Scheme, Settings.HttpProtocol, Request.Url.Port);
            //var fLink = String.Format("{1}{0}", "/news/sitemap.xml", builder.Uri.ToString().TrimEnd('/'));
            //content += "Sitemap: " + fLink + Environment.NewLine;
            //fLink = String.Format("{1}{0}", "/sitemap.xml", builder.Uri.ToString().TrimEnd('/'));
            //content += "Sitemap: " + fLink + Environment.NewLine;
            //content += "User-agent: *" + Environment.NewLine;


            String siteStatus = Settings.GetConfigString("SiteStatus", "dev");

            var builder = new UriBuilder(Settings.HttpProtocol, Request.Url.Host);
            var fLink = String.Format("{1}{0}", "/sitemap.xml", builder.Uri.ToString().TrimEnd('/'));
            content += "Sitemap: " + fLink + Environment.NewLine;


            if (string.Equals(siteStatus, "live", StringComparison.InvariantCultureIgnoreCase))
            {
                content += "Disallow: /Account/ " + Environment.NewLine;
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