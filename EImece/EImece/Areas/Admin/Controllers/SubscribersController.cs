using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class SubscribersController : BaseAdminController
    {
        // GET: Admin/Subscribers
        public ActionResult Index(String search="")
        {
            var subs = SubscriberRepository.GetAll();
            if (!String.IsNullOrEmpty(search))
            {
                subs = subs.Where(r => r.Name.ToLower().Contains(search));
            }
            var resultSubs = subs.ToList();
            return View(resultSubs);
        }
    }
}