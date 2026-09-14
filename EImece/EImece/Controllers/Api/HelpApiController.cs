using EImece.Domain.Caching;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace EImece.Controllers.Api
{
    public class HelpFeedbackRequest
    {
        public string PageKey { get; set; }
        public bool IsHelpful { get; set; }
        public string Comment { get; set; }
    }

    [RoutePrefix("api/help")]
    public class HelpApiController : ApiController
    {
        private readonly ISettingService _settingService;
        private readonly IEimeceCacheProvider _cacheProvider;
        private readonly ILogger<HelpApiController> _logger;

        public HelpApiController(ISettingService settingService, ILogger<HelpApiController> logger, IEimeceCacheProvider cacheProvider = null)
        {
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheProvider = cacheProvider;
        }

        /// <summary>
        /// Submits user feedback (thumbs up / thumbs down) for a page help guide.
        /// </summary>
        [HttpPost]
        [Route("feedback")]
        public IHttpActionResult SubmitFeedback([FromBody] HelpFeedbackRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PageKey))
            {
                return BadRequest("Invalid feedback request.");
            }

            string cleanKey = PageHelpHelper.ResolveKey(request.PageKey);
            _logger.LogInformation(
                "Help guide feedback received: Key={PageKey}, ResolvedKey={ResolvedKey}, IsHelpful={IsHelpful}, Comment={Comment}",
                request.PageKey,
                cleanKey,
                request.IsHelpful,
                request.Comment
            );

            return Ok(new
            {
                success = true,
                message = "Geri bildiriminiz için teşekkür ederiz!",
                pageKey = cleanKey,
                isHelpful = request.IsHelpful
            });
        }

        /// <summary>
        /// Alternate query-string format: GET /api/help?pageKey=change-password
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetHelpQuery([FromUri] string pageKey = null)
        {
            if (string.IsNullOrWhiteSpace(pageKey))
            {
                return BadRequest("pageKey query parameter is required.");
            }

            return await GetHelp(pageKey).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves page help content by key or slug (e.g. /api/help/change-password) reading from cache.
        /// </summary>
        [HttpGet]
        [Route("{*pageKey}")]
        public async Task<IHttpActionResult> GetHelp(string pageKey)
        {
            if (string.IsNullOrWhiteSpace(pageKey))
            {
                return BadRequest("pageKey cannot be empty.");
            }

            string cleanKey = PageHelpHelper.ResolveKey(pageKey);
            var item = await PageHelpHelper.GetHelpItemAsync(cleanKey, _settingService, _cacheProvider).ConfigureAwait(false);

            string title = item?.PageName;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = PageHelpHelper.GetPageTitle(cleanKey);
            }

            string content = PageHelpHelper.IsValidContent(item?.CurrentContent)
                ? item.CurrentContent
                : (PageHelpHelper.IsValidContent(item?.DefaultContent) ? item.DefaultContent : string.Empty);
            bool hasContent = PageHelpHelper.IsValidContent(content);
            string lastUpdated = item?.UpdatedDate != null ? item.UpdatedDate.Value.ToString("dd.MM.yyyy") : null;

            return Ok(new
            {
                success = true,
                pageKey = pageKey,
                resolvedKey = cleanKey,
                title = title,
                content = content,
                lastUpdated = lastUpdated,
                hasContent = hasContent
            });
        }
    }
}
