using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Drawing.Printing;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    //[AuthorizeRoles(Constants.AdministratorRole)]  // NOT ALLOWED TO GET THAT PAGES
    [RoutePrefix(Constants.StoriesCategoriesControllerRoutingPrefix)]
    public class StoriesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IStoryService StoryService { get; set; }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Index(int page = 1)
        {
            try
            {
                var stories = StoryService.GetMainPageStories(page, CurrentLanguage);
                stories.RecordPerPage = AppConfig.StoryDefaultRecordPerPage;
                stories.Page = page;
                return View(stories);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " page:" + page);
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Detail(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var storyId = id.GetId();
                var story = StoryService.GetStoryDetailViewModel(storyId);
                ViewBag.SeoId = story.Story.GetSeoUrl();
                return View(story);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " id:" + id);
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Categories(string id, int page = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var storyCategoryId = id.GetId();
                var storyCategory = StoryService.GetStoryCategoriesViewModel(storyCategoryId, page);
                storyCategory.RecordPerPage = AppConfig.StoryDefaultRecordPerPage;
                storyCategory.Page = page;
                ViewBag.SeoId = storyCategory.StoryCategory.GetSeoUrl();
                return View(storyCategory);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " id:" + id);
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Tag(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var tagId = id.GetId();
                int pageIndex = 1;
                int pageSize = AppConfig.StoryDefaultRecordPerPage;
                var stories = StoryService.GetStoriesByTagId(tagId, pageIndex, pageSize, CurrentLanguage);
                ViewBag.SeoId = stories.Tag.GetSeoUrl();
                return View(stories);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " id:" + id);
                return RedirectToAction("InternalServerError", "Error");
            }
        }
        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Author(string id, int page = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var authorName = id;
                int pageSize = AppConfig.StoryDefaultRecordPerPage;
                var stories = StoryService.GetStoriesByAuthorName(authorName, page, CurrentLanguage);
                stories.RecordPerPage = AppConfig.StoryDefaultRecordPerPage;
                stories.Page = page;
                ViewBag.SeoId = authorName+" "+Resources.Resource.Stories;
                return View(stories);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " id:" + id);
                return RedirectToAction("InternalServerError", "Error");
            }
        }
    }
}