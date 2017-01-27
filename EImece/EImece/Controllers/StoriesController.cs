using EImece.Domain;
using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class StoriesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Index(int page = 1)
        {
            var stories = StoryService.GetMainPageStories(page,  CurrentLanguage);
            return View(stories);
        }
        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Detail(String id)
        {
            var storyId = id.GetId();
            var story = StoryService.GetStoryDetailViewModel(storyId);
            return View(story);
        }
    }
}