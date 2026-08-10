using EImece.Domain.Entities;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class SubscribersController : BaseAdminController
    {
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<Subscriber, bool>> whereLambda = r => r.Name.Contains(search) || r.Email.Contains(search);
            var subs = await SubscriberService.SearchEntitiesAsync(whereLambda, search, null);
            return View(subs);
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync(CancellationToken cancellationToken, string format = "excel")
        {
            return await DownloadFileAsync(format);
        }

        private async Task<ActionResult> DownloadFileAsync(string format = "excel")
        {
            var subscibers = await SubscriberService.GetAllAsync();

            var result = from r in subscibers
                         select new
                         {
                             r.Name,
                             r.Email,
                             r.CreatedDate,
                             r.Note
                         };

            return DownloadFile(result, String.Format("subscibers-{0}", GetCurrentLanguage), format);
        }
    }
}
