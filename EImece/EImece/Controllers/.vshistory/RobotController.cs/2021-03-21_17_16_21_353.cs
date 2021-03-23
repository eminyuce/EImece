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
            var content = "";
            if (AppConfig.IsSiteUnderConstruction)
            {
                content += "Disallow: /" + Environment.NewLine;
                content += "# Disallow Robots (Debug)" + Environment.NewLine;
            }
            else if (AppConfig.IsSiteLive)
            {
                
                    var builder = new UriBuilder(AppConfig.HttpProtocol, Request.Url.Host, Request.Url.Port);
                    builder.Path += "sitemap.xml";
                    var fLink = builder.Uri;
                    content += "User-agent:* " + Environment.NewLine;
                    content += "Sitemap: " + fLink + Environment.NewLine;

                    content += "Disallow: /Ajax/ " + Environment.NewLine;
                    content += "Disallow: /Error/ " + Environment.NewLine;
                    content += "Disallow: /Manage/ " + Environment.NewLine;
                    content += "Disallow: /Account/ " + Environment.NewLine;
                    content += "# Allow Robots (Release)" + Environment.NewLine;
 

        } else
                {
                    content = "Disallow: /" + Environment.NewLine;
                    content += "# Disallow Robots (Debug)" + Environment.NewLine;
                }
            return File(Encoding.UTF8.GetBytes(content), TextPlain);
        }
    }
}