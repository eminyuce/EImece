using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class StoriesController : BaseController
    {
        // GET: Stories
        public ActionResult Index()
        {
            var stories = StoryRepository.GetAll().ToList();
            return View(stories);
        }
    }
}