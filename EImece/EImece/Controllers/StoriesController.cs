using EImece.Web.Controllers;
using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Web.Filters;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    // [AuthorizeRoles(Constants.AdministratorRole)]  // NOT ALLOWED TO GET THAT PAGES
    [RoutePrefix(Constants.StoriesCategoriesControllerRoutingPrefix)]
    public class StoriesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IStoryService StoryService;

        public StoriesController(
            ISettingService settingService,
            AutoMapper.IMapper mapper,
            IStoryService storyService)
            : base(settingService, mapper)
        {
            StoryService = storyService ?? throw new ArgumentNullException(nameof(storyService));
        }

        [Route("")]
        [Route("~/stories")]
        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, int page = 1)
        {
            Logger.Debug($"Entering Index action with page: {page}");
            try
            {
                var stories = await StoryService.GetMainPageStoriesAsync(page, CurrentLanguage, cancellationToken);
                Logger.Debug($"Retrieved {stories?.StorefrontStories?.Count ?? 0} stories for page: {page}, language: {CurrentLanguage}");
                Logger.Debug("Returning Index view.");
                return View(stories);
            }
            catch (Exception ex)
            {
                return HandleUnexpectedError(ex, $"Exception in Index action for page: {page}. Message: {ex.Message}");
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Detail(CancellationToken cancellationToken, String id)
        {
            Logger.Debug($"Entering Detail action with id: '{id}'");
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    Logger.Error("Story ID is null or empty.");
                    Logger.Debug("Returning BadRequest status.");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var storyId = id.GetId();
                Logger.Debug($"Parsed story ID: {storyId}");

                var story = await StoryService.GetStoryDetailViewModelAsync(storyId, cancellationToken);
                if (story == null || story.StorefrontStory == null)
                {
                    return HttpNotFound();
                }
                Logger.Debug($"Retrieved story details for ID: {storyId}, Name: {story.StorefrontStory.Name}");

                ViewBag.SeoId = story.StorefrontStory.SeoUrl;
                Logger.Debug($"Set ViewBag.SeoId: {ViewBag.SeoId}");

                Logger.Debug("Returning Detail view.");
                return View(story);
            }
            catch (Exception ex)
            {
                return HandleUnexpectedError(ex, $"Exception in Detail action for id: '{id}'. Message: {ex.Message}");
            }
        }

        [Route(Constants.StoryCategoryPrefix)]
        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Categories(CancellationToken cancellationToken, String id, int page = 1)
        {
            Logger.Debug($"Entering Categories action with id: '{id}', page: {page}");
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    Logger.Error("Story category ID is null or empty.");
                    Logger.Debug("Returning BadRequest status.");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var storyCategoryId = id.GetId();
                Logger.Debug($"Parsed story category ID: {storyCategoryId}");

                var storyCategory = await StoryService.GetStoryCategoriesViewModelAsync(storyCategoryId, page, cancellationToken);
                if (storyCategory == null || storyCategory.StoryCategory == null)
                {
                    return HttpNotFound();
                }
                Logger.Debug($"Retrieved story category for ID: {storyCategoryId}, Name: {storyCategory.StoryCategory.Name}, Stories Count: {storyCategory.StorefrontStories?.Count ?? 0}");

                ViewBag.SeoId = storyCategory.StoryCategory.GetSeoUrl();
                Logger.Debug($"Set ViewBag.SeoId: {ViewBag.SeoId}");

                Logger.Debug("Returning Categories view.");
                return View(storyCategory);
            }
            catch (Exception ex)
            {
                return HandleUnexpectedError(ex, $"Exception in Categories action for id: '{id}'. Message: {ex.Message}");
            }
        }

        /// <summary>
        /// Permanent redirect from legacy /s/categories/{id} to /s/sc/{id}.
        /// </summary>
        public ActionResult CategoriesLegacy(String id, int page = 1)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var destination = page > 1
                ? Url.RouteUrl("Storycategories", new { id, page })
                : Url.RouteUrl("Storycategories", new { id });
            if (String.IsNullOrEmpty(destination))
            {
                destination = page > 1
                    ? Url.Action("Categories", "Stories", new { id, page })
                    : Url.Action("Categories", "Stories", new { id });
            }

            return RedirectPermanent(destination);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Tag(CancellationToken cancellationToken, String id)
        {
            Logger.Debug($"Entering Tag action with id: '{id}'");
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    Logger.Error("Tag ID is null or empty.");
                    Logger.Debug("Returning BadRequest status.");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var tagId = id.GetId();
                Logger.Debug($"Parsed tag ID: {tagId}");

                int pageIndex = 1;
                int pageSize = 20;
                Logger.Debug($"Using pageIndex: {pageIndex}, pageSize: {pageSize}");

                var stories = await StoryService.GetStoriesByTagIdAsync(tagId, pageIndex, pageSize, CurrentLanguage, cancellationToken);
                if (stories == null || stories.Tag == null)
                {
                    Logger.Warn($"Tag not found for id: '{id}' (parsed: {tagId})");
                    return RedirectToAction("NotFound", "Error");
                }

                if (stories.StoryTags == null || stories.StoryTags.TotalCount == 0)
                {
                    Logger.Warn($"Tag '{id}' has no associated stories; redirecting to NotFound.");
                    return RedirectToAction("NotFound", "Error");
                }

                Logger.Debug($"Retrieved {stories.StoryTags.Count} stories for tag ID: {tagId}, language: {CurrentLanguage}");

                ViewBag.SeoId = stories.Tag.GetSeoUrl();
                Logger.Debug($"Set ViewBag.SeoId: {ViewBag.SeoId}");

                Logger.Debug("Returning Tag view.");
                return View(stories);
            }
            catch (Exception ex)
            {
                return HandleUnexpectedError(ex, $"Exception in Tag action for id: '{id}'. Message: {ex.Message}");
            }
        }
    }
}
