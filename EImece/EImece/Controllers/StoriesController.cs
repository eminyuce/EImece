using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Net;
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
        public ActionResult Index(int page = 1)
        {
            Logger.Info($"Entering Index action with page: {page}");
            try
            {
                var stories = StoryService.GetMainPageStoriesDto(page, CurrentLanguage);
                Logger.Info($"Retrieved {stories?.Stories?.Count ?? 0} stories for page: {page}, language: {CurrentLanguage}");
                Logger.Info("Returning Index view.");
                return View(stories);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Exception in Index action for page: {page}. Message: {ex.Message}");
                Logger.Info("Redirecting to InternalServerError error page.");
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Detail(String id)
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

                var story = StoryService.GetStoryDetailViewModelDto(storyId);
                Logger.Info($"Retrieved story details for ID: {storyId}, Name: {story?.Story?.Name}");

                ViewBag.SeoId = story.Story.GetSeoUrl();
                Logger.Info($"Set ViewBag.SeoId: {ViewBag.SeoId}");

                Logger.Info("Returning Detail view.");
                return View(story);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Exception in Detail action for id: '{id}'. Message: {ex.Message}");
                Logger.Info("Redirecting to InternalServerError error page.");
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Categories(String id, int page = 1)
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

                var storyCategory = StoryService.GetStoryCategoriesViewModelDto(storyCategoryId, page);
                Logger.Info($"Retrieved story category for ID: {storyCategoryId}, Name: {storyCategory?.StoryCategory?.Name}, Stories Count: {storyCategory?.Stories?.Count ?? 0}");

                ViewBag.SeoId = storyCategory.StoryCategory.GetSeoUrl();
                Logger.Info($"Set ViewBag.SeoId: {ViewBag.SeoId}");

                Logger.Info("Returning Categories view.");
                return View(storyCategory);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Exception in Categories action for id: '{id}'. Message: {ex.Message}");
                Logger.Info("Redirecting to InternalServerError error page.");
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Tag(String id)
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

                var stories = StoryService.GetStoriesByTagIdDto(tagId, pageIndex, pageSize, CurrentLanguage);
                Logger.Info($"Retrieved {stories.StoryTags.Count} stories for tag ID: {tagId}, language: {CurrentLanguage}");

                ViewBag.SeoId = stories.Tag.GetSeoUrl();
                Logger.Info($"Set ViewBag.SeoId: {ViewBag.SeoId}");

                Logger.Info("Returning Tag view.");
                return View(stories);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Exception in Tag action for id: '{id}'. Message: {ex.Message}");
                Logger.Info("Redirecting to InternalServerError error page.");
                return RedirectToAction("InternalServerError", "Error");
            }
        }
    }
}