using EImece.Controllers;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Web.Mvc;

namespace EImece.Areas.Amp.Controllers
{
    public class StoriesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IStoryService StoryService { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Detail(String id)
        {
            try
            {
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
    }
}