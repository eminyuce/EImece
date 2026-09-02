using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using EImece.Web.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EImece.Controllers
{
    [AuthorizeRoles(Constants.AdministratorRole)]  // NOT ALLOWED TO GET THAT PAGES
    public class UrlController : ApiController
    {
        private readonly ILogger<UrlController> _logger;

        private readonly IShortUrlService _shortUrlService;

        public UrlController(IShortUrlService shortUrlService, ILogger<UrlController> logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _shortUrlService = shortUrlService ?? throw new ArgumentNullException(nameof(shortUrlService));
        }

        [HttpGet]
        [Route("u/{key}")]
        public HttpResponseMessage Get(string key)
        {
            _logger.LogInformation("Get key:" + key);
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            var shortUrlObj = _shortUrlService.GetShortUrlByKey(key);
            if (shortUrlObj != null)
            {
                Uri safeUri;
                if (!SecurityHelper.IsSafeHttpRedirectUrl(shortUrlObj.Url, out safeUri))
                {
                    _logger.LogWarning("Blocked unsafe short URL redirect for key: " + key);
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                response.Headers.Location = safeUri;
                return response;
            }

            return null;
        }

        [HttpPost]
        [Route("short")]
        public HttpResponseMessage Post([FromBody] String url, [FromBody] String email = "", [FromBody] String groupName = "")
        {
            _logger.LogInformation("Post Short:" + url);
            return Request.CreateResponse(HttpStatusCode.OK, _shortUrlService.GenerateShortUrl(url, email, groupName));
        }
    }
}