using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.SiteMap;
using EImece.Domain.Services;
using EImece.Domain.DependencyInjection;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Text;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class SiteMapController : BaseController
    {
        [Inject]
        public SiteMapService SiteMapService { get; set; }

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