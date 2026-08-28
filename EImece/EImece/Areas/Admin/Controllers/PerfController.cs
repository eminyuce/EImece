using EImece.Areas.Admin.Models;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Observability.Metrics;
using EImece.Domain.Services.IServices;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DomainConstants = EImece.Domain.Constants;

namespace EImece.Areas.Admin.Controllers
{
    /// <summary>
    /// Admin performance metrics controller providing local in-memory visibility of [Timed] execution stats.
    /// Accessible only to administrators.
    /// </summary>
    [AuthorizeRoles(DomainConstants.AdministratorRole)]
    public class PerfController : BaseAdminController
    {
        public PerfController(ISettingService settingService)
            : base(settingService)
        {
        }

        // GET: Admin/Perf
        [HttpGet]
        public ActionResult Index(string search = "", string type = "all")
        {
            var snapshots = FilterSnapshots(search, type);

            var viewModel = new PerfIndexViewModel
            {
                Stats = snapshots,
                SearchTerm = search,
                SelectedType = type
            };

            return View(viewModel);
        }

        // GET/POST: Admin/Perf/IndexGrid
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult IndexGrid(string search = "", string type = "all")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search, type });
            }

            var snapshots = FilterSnapshots(search, type);
            return new QueryableResult<PerfStatSnapshot>(snapshots.AsQueryable());
        }

        // GET: Admin/Perf/ExportExcel
        [HttpGet, ActionName("ExportExcel")]
        public ActionResult ExportExcel(string format = "excel", string search = "", string type = "all")
        {
            var snapshots = FilterSnapshots(search, type);

            var result = from s in snapshots
                         select new
                         {
                             MetricName = s.Name,
                             Type = GetTypeLabel(s.Name),
                             Calls = s.Count,
                             AvgMs = Math.Round(s.AvgMs, 2),
                             MinMs = Math.Round(s.MinMs, 2),
                             MaxMs = Math.Round(s.MaxMs, 2),
                             LastMs = Math.Round(s.LastMs, 2),
                             TotalDurationSec = Math.Round(s.SumMs / 1000.0, 2),
                             LastInvokedUtc = s.LastUtc.ToString("yyyy-MM-dd HH:mm:ss")
                         };

            var dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            return DownloadFile(result, $"perf-stats-{dateStr}", format);
        }

        // POST: Admin/Perf/ClearStats
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearStats()
        {
            PerfStats.Clear();
            TempData[DomainConstants.StatusMessageKey] = Resources.AdminResource.ClearPerfStatsTitle;
            return RedirectToAction("Index");
        }

        private static List<PerfStatSnapshot> FilterSnapshots(string search, string type)
        {
            var snapshots = PerfStats.Snapshot();

            if (!string.IsNullOrWhiteSpace(search))
            {
                snapshots = snapshots
                    .Where(x => x.Name != null && x.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(type) && !string.Equals(type, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(type, "controller", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "app", StringComparison.OrdinalIgnoreCase))
                {
                    snapshots = snapshots.Where(x => x.Name != null && x.Name.StartsWith("app.", StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else if (string.Equals(type, "service", StringComparison.OrdinalIgnoreCase))
                {
                    snapshots = snapshots.Where(x => x.Name != null && x.Name.StartsWith("service.", StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else if (string.Equals(type, "repo", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "repository", StringComparison.OrdinalIgnoreCase))
                {
                    snapshots = snapshots.Where(x => x.Name != null && x.Name.StartsWith("repo.", StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else if (string.Equals(type, "other", StringComparison.OrdinalIgnoreCase))
                {
                    snapshots = snapshots.Where(x => x.Name != null &&
                        !x.Name.StartsWith("app.", StringComparison.OrdinalIgnoreCase) &&
                        !x.Name.StartsWith("service.", StringComparison.OrdinalIgnoreCase) &&
                        !x.Name.StartsWith("repo.", StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }

            return snapshots;
        }

        private static string GetTypeLabel(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Other";
            if (name.StartsWith("app.", StringComparison.OrdinalIgnoreCase)) return "Controller";
            if (name.StartsWith("service.", StringComparison.OrdinalIgnoreCase)) return "Service";
            if (name.StartsWith("repo.", StringComparison.OrdinalIgnoreCase)) return "Repository";
            return "Other";
        }
    }
}
