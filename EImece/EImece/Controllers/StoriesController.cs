using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
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

        [Inject]
        public IStoryService StoryService { get; set; }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, int page = 1)
        {
            Logger.Info($"Entering Index action with page: {page}");
            try
            {
                var stories = await StoryService.GetMainPageStoriesAsync(page, CurrentLanguage, cancellationToken);
                Logger.Info($"Retrieved {stories?.Stories?.Count ?? 0} stories for page: {page}, language: {CurrentLanguage}");
                Logger.Info("Returning Index view.");
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
            Logger.Info($"Entering Detail action with id: '{id}'");
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    Logger.Error("Story ID is null or empty.");
                    Logger.Info("Returning BadRequest status.");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var storyId = id.GetId();
                Logger.Info($"Parsed story ID: {storyId}");

                var story = await StoryService.GetStoryDetailViewModelAsync(storyId, cancellationToken);
                Logger.Info($"Retrieved story details for ID: {storyId}, Name: {story?.Story?.Name}");

                ViewBag.SeoId = story.Story.GetSeoUrl();
                Logger.Info($"Set ViewBag.SeoId: {ViewBag.SeoId}");

                Logger.Info("Returning Detail view.");
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
            Logger.Info($"Entering Categories action with id: '{id}', page: {page}");
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    Logger.Error("Story category ID is null or empty.");
                    Logger.Info("Returning BadRequest status.");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var storyCategoryId = id.GetId();
                Logger.Info($"Parsed story category ID: {storyCategoryId}");

                var storyCategory = await StoryService.GetStoryCategoriesViewModelAsync(storyCategoryId, page, cancellationToken);
                Logger.Info($"Retrieved story category for ID: {storyCategoryId}, Name: {storyCategory?.StoryCategory?.Name}, Stories Count: {storyCategory?.Stories?.Count ?? 0}");

                ViewBag.SeoId = storyCategory.StoryCategory.GetSeoUrl();
                Logger.Info($"Set ViewBag.SeoId: {ViewBag.SeoId}");

                Logger.Info("Returning Categories view.");
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
            Logger.Info($"Entering Tag action with id: '{id}'");
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    Logger.Error("Tag ID is null or empty.");
                    Logger.Info("Returning BadRequest status.");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var tagId = id.GetId();
                Logger.Info($"Parsed tag ID: {tagId}");

                int pageIndex = 1;
                int pageSize = 20;
                Logger.Info($"Using pageIndex: {pageIndex}, pageSize: {pageSize}");

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

                Logger.Info($"Retrieved {stories.StoryTags.Count} stories for tag ID: {tagId}, language: {CurrentLanguage}");

                ViewBag.SeoId = stories.Tag.GetSeoUrl();
                Logger.Info($"Set ViewBag.SeoId: {ViewBag.SeoId}");

                Logger.Info("Returning Tag view.");
                return View(stories);
            }
            catch (Exception ex)
            {
                return HandleUnexpectedError(ex, $"Exception in Tag action for id: '{id}'. Message: {ex.Message}");
            }
        }
    }
}
