using EImece.Domain.Repositories.IRepositories;
using Ninject;
using NLog;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EImece.Controllers
{
    [AuthorizeRoles(Constants.AdministratorRole)]  // NOT ALLOWED TO GET THAT PAGES
    public class UrlController : ApiController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IShortUrlRepository ShortUrlRepository { get; set; }

        [HttpGet]
        [Route("{key}")]
        public HttpResponseMessage Get(string key)
        {
            Logger.Info("Get key:" + key);
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            var shortUrlObj = ShortUrlRepository.GetShortUrlByKey(key);
            if (shortUrlObj != null)
            {
                response.Headers.Location = new Uri(shortUrlObj.Url);
                return response;
            }

            return null;
        }

        [HttpPost]
        [Route("short")]
        public HttpResponseMessage Post([FromBody] String url, [FromBody] String email = "", [FromBody] String groupName = "")
        {
            Logger.Info("Post Short:" + url);
            return Request.CreateResponse(HttpStatusCode.OK, ShortUrlRepository.GenerateShortUrl(url, email, groupName));
        }
    }
}