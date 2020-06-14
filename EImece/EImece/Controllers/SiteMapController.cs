using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.SiteMap;
using EImece.Domain.Services;
using Ninject;
using NLog;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class SiteMapController : BaseController
    {
        [Inject]
        public SiteMapService SiteMapService { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [CustomOutputCache(CacheProfile = "Cache1Hour")]
        [Route("sitemap.xml")]
        public ActionResult Index()
        {
            return new SitemapResult(SiteMapService.GenerateSiteMap());
        }
    }
}