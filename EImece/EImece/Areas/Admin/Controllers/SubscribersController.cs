using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
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
            var subs = SubscriberService.SearchEntities(whereLambda, search, CurrentLanguage);
            return View(subs);
        }
        public ActionResult ExportSubscribers()
        {
            var subscibers = SubscriberService.GetAll().ToList();
            DataTable dt = new DataTable();
            dt.TableName = "subscibers";

            var result = from r in subscibers
                         select new
                         {
                             r.Name,
                             r.Email,
                             r.CreatedDate,
                             r.Note 
                         };
            dt = GeneralHelper.LINQToDataTable(result);

            var ms = ExcelHelper.GetExcelByteArrayFromDataTable(dt);
            return File(ms, "application/vnd.ms-excel",
                String.Format("Subscibers-{0}.xls",
                DateTime.Now.ToString("yyyy-MM-dd")));
        }
    }
}