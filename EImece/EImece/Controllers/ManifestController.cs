using EImece.Domain;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services.IServices;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [AllowAnonymous]
    public class ManifestController : Controller
    {
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        private readonly IWebAppManifestService _webAppManifestService;

        public ManifestController(IWebAppManifestService webAppManifestService)
        {
            _webAppManifestService = webAppManifestService ?? throw new ArgumentNullException(nameof(webAppManifestService));
        }

        [HttpGet]
        [Route("manifest.json")]
        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public async Task<ActionResult> Index()
        {
            var json = await _webAppManifestService.GetManifestJsonAsync().ConfigureAwait(false);
            return Content(json, Constants.WebAppManifestContentType, Utf8NoBom);
        }
    }
}
