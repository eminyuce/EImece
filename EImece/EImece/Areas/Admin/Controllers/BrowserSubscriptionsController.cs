using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class BrowserSubscriptionsController : BaseAdminController
    {
        // GET: Admin/BrowserSubscriptions
        public ActionResult Index()
        {
            return View();
        }
    }
}