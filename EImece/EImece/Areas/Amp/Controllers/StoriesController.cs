using EImece.Domain;
using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Controllers;

namespace EImece.Areas.Amp.Controllers
{
    public class StoriesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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