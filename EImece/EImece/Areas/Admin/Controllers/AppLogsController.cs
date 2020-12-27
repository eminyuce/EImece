using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Repositories;
using NLog;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class AppLogsController : BaseAdminController
    {
        private AppLogRepository AppLogRepository;
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AppLogsController(AppLogRepository repository)
        {
            this.AppLogRepository = repository;
        }

        // GET: Admin/AppLogs
        public ActionResult Index(string search = "")
        {
            var logs = AppLogRepository.GetAppLogs(search);
            return View(logs);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            AppLogRepository.DeleteAppLog(id);
            return ReturnIndexIfNotUrlReferrer("Index");
        }

        [DeleteAuthorize()]
        public ActionResult RemoveAll(string eventLevel = "")
        {
            AppLogRepository.RemoveAll(eventLevel);
            return ReturnIndexIfNotUrlReferrer("Index");
        }
    }
}