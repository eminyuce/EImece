using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class BrowserSubscribersController : BaseAdminController
    {
        // GET: Admin/BrowserSubscribers
        public ActionResult Index()
        {
            var browserSubscribers = BrowserSubscriberService.GetAll();
            return View(browserSubscribers);
        }
    }
}