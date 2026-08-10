using EImece.Areas.Admin.Models;
using EImece.Domain.Observability.Metrics;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class MetricsController : BaseAdminController
    {
        private readonly IApplicationMetrics _applicationMetrics;

        public MetricsController(IApplicationMetrics applicationMetrics)
        {
            _applicationMetrics = applicationMetrics;
        }

        // GET: Admin/Metrics
        public ActionResult Index(string search = "")
        {
            var snapshots = _applicationMetrics.GetSnapshots();

            IQueryable<System.Collections.Generic.KeyValuePair<string, MetricSnapshot>> query = snapshots.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Key.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var metrics = query
                .OrderBy(x => x.Key)
                .Select(x => new MetricDisplayItem
                {
                    Name = x.Key,
                    Count = x.Value.Count,
                    ErrorCount = x.Value.ErrorCount,
                    SampleWindowSize = x.Value.SampleWindowSize,
                    AverageDurationMs = Math.Round(x.Value.AverageDurationMs, 2),
                    MinDurationMs = x.Value.MinDurationMs,
                    MaxDurationMs = x.Value.MaxDurationMs,
                    P50DurationMs = x.Value.P50DurationMs,
                    P75DurationMs = x.Value.P75DurationMs,
                    P90DurationMs = x.Value.P90DurationMs,
                    P95DurationMs = x.Value.P95DurationMs,
                    P99DurationMs = x.Value.P99DurationMs
                })
                .ToList();

            var viewModel = new MetricsIndexViewModel
            {
                Metrics = metrics,
                SearchTerm = search,
                PageNumber = 1,
                PageSize = 50,
                TotalCount = metrics.Count
            };

            return View(viewModel);
        }
    }
}
