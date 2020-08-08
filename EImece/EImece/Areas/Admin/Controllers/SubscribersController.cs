using EImece.Domain.Entities;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class SubscribersController : BaseAdminController
    {
        // GET: Admin/Subscribers
        public ActionResult Index(String search = "")
        {
            Expression<Func<Subscriber, bool>> whereLambda = r => r.Name.Contains(search) || r.Email.Contains(search);
            var subs = SubscriberService.SearchEntities(whereLambda, search, null);
            return View(subs);
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync()
        {
            return await Task.Run(() =>
            {
                return DownloadFile();
            }).ConfigureAwait(true);
        }

        private ActionResult DownloadFile()
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