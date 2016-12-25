using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class SubscribersController : BaseAdminController
    {
        // GET: Admin/Subscribers
        public ActionResult Index(String search="")
        {
            Expression<Func<Subscriber, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower()) || r.Email.ToLower().Contains(search.Trim().ToLower());
            var subs = SubsciberService.SearchEntities(whereLambda, search);
            return View(subs);
        }
    }
}