using Microsoft.Extensions.Logging;
using EImece.Domain;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers;
using EImece.Web.Filters;
using EImece.Domain.Services.IServices;
using System;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class RobotController : Controller
    {
        private readonly ILogger<RobotController> _logger;

        private readonly ISettingService SettingService;

        public RobotController(ISettingService settingService, ILogger<RobotController> logger)
         {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            SettingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
        }

        // GET: Robots
        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public async Task<FileContentResult> RobotsText()
        {
            var sb = new StringBuilder(512);

            ISettingService settingService = SettingService;
            if (settingService == null)
            {
                try
                {
                    settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                }
                catch
                {
                    settingService = null;
                }
            }

            var isUnderConstruction = settingService != null
                ? (await settingService.GetSettingByKeyAsync(Constants.IsSiteUnderConstruction)).ToBool(false)
                : false;

            if (!SeoSettings.AllowIndexing)
            {
                _logger.LogDebug("Search engine indexing is disabled. Setting robots.txt to disallow all.");
                sb.AppendLine(Constants.RobotsUserAgentAll)
                  .AppendLine("Disallow: /");
            }
            else if (isUnderConstruction || AppConfig.IsSiteUnderDevelopment)
            {
                _logger.LogDebug("Site is under construction or development. Setting robots.txt to disallow all.");
                sb.AppendLine(Constants.RobotsUserAgentAll)
                  .AppendLine("Disallow: /")
                  .AppendLine("# Disallow Robots (Debug)");
            }
            else if (AppConfig.IsSiteLive)
            {
                string host = !string.IsNullOrWhiteSpace(AppConfig.Domain) ? AppConfig.Domain : (Request?.Url != null ? Request.Url.Authority : "localhost");
                string protocol = !string.IsNullOrWhiteSpace(AppConfig.HttpProtocol) ? AppConfig.HttpProtocol : "https";
                string sitemapUrl = $"{protocol}://{host}/sitemap.xml";

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
                sb.AppendLine(Constants.RobotsUserAgentAll)
                  .AppendLine("Allow: /");
            }

            return await Task.FromResult(File(Encoding.UTF8.GetBytes(sb.ToString()), MediaTypeNames.Text.Plain));
        }
    }
}
