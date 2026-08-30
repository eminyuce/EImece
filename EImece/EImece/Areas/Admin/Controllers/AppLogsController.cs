using EImece.Web.Areas.Admin.Controllers;
using EImece.Domain.Entities;
using EImece.Web.Filters;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class AppLogsController : BaseAdminController
    {
        private static readonly string[] KnownEventLevels =
        {
            "Trace", "Debug", "Info", "Warn", "Error", "Fatal"
        };

        private readonly IAppLogService AppLogService;
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AppLogsController(
            ISettingService settingService,
            IAppLogService appLogService)
            : base(settingService)
        {
            this.AppLogService = appLogService ?? throw new ArgumentNullException(nameof(appLogService));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, string search = "", string eventLevel = "")
        {
            eventLevel = NormalizeEventLevel(eventLevel);
            ViewBag.Search = search ?? string.Empty;
            ViewBag.EventLevel = eventLevel;
            ViewBag.EventLevels = KnownEventLevels;
            var logs = await AppLogService.GetAppLogsAsync(search, eventLevel, cancellationToken);
            return View(logs);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, string search = "", string eventLevel = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search, eventLevel });
            }

            eventLevel = NormalizeEventLevel(eventLevel);
            var logs = await AppLogService.GetAppLogsAsync(search, eventLevel, cancellationToken);
            return new QueryableResult<AppLog>(logs.AsQueryable());
        }

        [HttpGet]
        public async Task<ActionResult> Download(CancellationToken cancellationToken, string search = "", string eventLevel = "")
        {
            eventLevel = NormalizeEventLevel(eventLevel);
            var logs = await AppLogService.GetAppLogsAsync(search, eventLevel, cancellationToken);
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
        public async Task<ActionResult> ExportExcel(CancellationToken cancellationToken, string format = "excel", string search = "", string eventLevel = "")
        {
            eventLevel = NormalizeEventLevel(eventLevel);
            var logs = await AppLogService.GetAppLogsAsync(search, eventLevel, cancellationToken);
            var levelPart = string.IsNullOrEmpty(eventLevel) ? "all" : eventLevel;
            return DownloadFile(logs, string.Format("AppLogs-{0}", levelPart), format);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            await AppLogService.DeleteAppLogAsync(id);
            SetSuccessMessage();
            return ReturnIndexIfNotUrlReferrer("Index");
        }

        [DeleteAuthorize()]
        public async Task<ActionResult> RemoveAll(CancellationToken cancellationToken, string eventLevel = "")
        {
            eventLevel = NormalizeEventLevel(eventLevel);
            await AppLogService.RemoveAllAsync(eventLevel, cancellationToken);
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
