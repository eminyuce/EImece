using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Web.Controllers;
using EImece.Web.Filters;
using EImece.Web.Infrastructure.ActionResults;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class SiteMapController : BaseController
    {
        private readonly SiteMapService SiteMapService;

        public SiteMapController(
            ISettingService settingService,
            AutoMapper.IMapper mapper,
            SiteMapService siteMapService,
            ILogger<SiteMapController> logger)
            : base(settingService, mapper, logger)
        {
            SiteMapService = siteMapService ?? throw new ArgumentNullException(nameof(siteMapService));
        }

        [CustomOutputCache(CacheProfile = Constants.Cache1Hour)]
        [Route("sitemap.xml")]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var isUnderConstruction = SettingService != null
                ? (await SettingService.GetSettingByKeyAsync(Constants.IsSiteUnderConstruction)).ToBool(false)
                : false;

            if (isUnderConstruction)
            {
                return File(Encoding.UTF8.GetBytes(""), MediaTypeNames.Text.Plain);
            }
            return new SitemapResult(await SiteMapService.GenerateSiteMapAsync());
        }
    }
}