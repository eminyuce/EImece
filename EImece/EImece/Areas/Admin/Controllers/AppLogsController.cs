using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Repositories;
using NLog;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class AppLogsController : BaseAdminController
    {
        private static readonly string[] KnownEventLevels =
        {
            "Trace", "Debug", "Info", "Warn", "Error", "Fatal"
        };

        private AppLogRepository AppLogRepository;
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AppLogsController(AppLogRepository repository)
        {
            this.AppLogRepository = repository;
        }

        // GET: Admin/AppLogs
        public ActionResult Index(string search = "", string eventLevel = "")
        {
            eventLevel = NormalizeEventLevel(eventLevel);
            ViewBag.Search = search ?? string.Empty;
            ViewBag.EventLevel = eventLevel;
            ViewBag.EventLevels = KnownEventLevels;
            var logs = AppLogRepository.GetAppLogs(search, eventLevel);
            return View(logs);
        }

        /// <summary>
        /// Download filtered logs as a UTF-8 text file (same content as the textarea view).
        /// </summary>
        [HttpGet]
        public ActionResult Download(string search = "", string eventLevel = "")
        {
            eventLevel = NormalizeEventLevel(eventLevel);
            var logs = AppLogRepository.GetAppLogs(search, eventLevel);
            var sb = new StringBuilder(Math.Max(256, logs.Count * 128));
            foreach (var log in logs)
            {
                sb.Append(log.ToLogStr());
            }

            var levelPart = string.IsNullOrEmpty(eventLevel) ? "all" : eventLevel.ToLowerInvariant();
            var fileName = string.Format("applogs-{0}-{1:yyyy-MM-dd-HHmm}.txt", levelPart, DateTime.Now);
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var bytes = utf8.GetBytes(sb.ToString());
            return File(bytes, "text/plain; charset=utf-8", fileName);
        }

        [HttpGet, ActionName("ExportExcel")]
        public ActionResult ExportExcel(string format = "excel", string search = "", string eventLevel = "")
        {
            eventLevel = NormalizeEventLevel(eventLevel);
            var logs = AppLogRepository.GetAppLogs(search, eventLevel);
            var levelPart = string.IsNullOrEmpty(eventLevel) ? "all" : eventLevel;
            return DownloadFile(logs, string.Format("AppLogs-{0}", levelPart), format);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            AppLogRepository.DeleteAppLog(id);
            SetSuccessMessage();
            return ReturnIndexIfNotUrlReferrer("Index");
        }

        [DeleteAuthorize()]
        public ActionResult RemoveAll(string eventLevel = "")
        {
            eventLevel = NormalizeEventLevel(eventLevel);
            AppLogRepository.RemoveAll(eventLevel);
            SetSuccessMessage();
            return ReturnIndexIfNotUrlReferrer("Index");
        }

        private static string NormalizeEventLevel(string eventLevel)
        {
            if (string.IsNullOrWhiteSpace(eventLevel))
            {
                return string.Empty;
            }

            var trimmed = eventLevel.Trim();
            var known = KnownEventLevels.FirstOrDefault(l =>
                string.Equals(l, trimmed, StringComparison.OrdinalIgnoreCase));
            return known ?? trimmed;
        }
    }
}
