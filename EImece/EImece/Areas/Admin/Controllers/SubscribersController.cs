using EImece.Domain.Entities;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class SubscribersController : BaseAdminController
    {
        // GET: Admin/Subscribers
        public ActionResult Index(String search = "")
        {
            Expression<Func<Subscriber, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower()) || r.Email.ToLower().Contains(search.Trim().ToLower());
            var subs = SubscriberService.SearchEntities(whereLambda, search, null);
            return View(subs);
        }
        public ActionResult ExportExcel()
        {
            var subscibers = SubscriberService.GetAll().ToList();


            var result = from r in subscibers
                         select new
                         {
                             r.Name,
                             r.Email,
                             r.CreatedDate,
                             r.Note
                         };

            return DownloadFile(result, String.Format("subscibers-{0}", GetCurrentLanguage));
        }
    }
}