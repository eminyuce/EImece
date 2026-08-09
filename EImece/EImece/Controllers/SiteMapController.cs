using EImece.Domain;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.SiteMap;
using EImece.Domain.Services;
using EImece.Domain.DependencyInjection;
using System.Threading.Tasks;
using System.Text;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class SiteMapController : BaseController
    {
        private const string TextPlain = "text/plain";

        [Inject]
        public SiteMapService SiteMapService { get; set; }

        [CustomOutputCache(CacheProfile = Constants.Cache1Hour)]
        [Route("sitemap.xml")]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            if (AppConfig.IsSiteUnderConstruction)
            {
                return File(Encoding.UTF8.GetBytes(""), TextPlain);
            }
            return new SitemapResult(await SiteMapService.GenerateSiteMapAsync());
        }
    }
}