using EImece.Web.Areas.Admin.Controllers;
using EImece.Areas.Admin.Models;
using EImece.Domain.Observability.Metrics;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class MetricsController : BaseAdminController
    {
        private readonly IApplicationMetrics _applicationMetrics;

        public MetricsController(
            ISettingService settingService,
            IApplicationMetrics applicationMetrics)
            : base(settingService)
        {
            _applicationMetrics = applicationMetrics ?? throw new ArgumentNullException(nameof(applicationMetrics));
        }

        // GET: Admin/Metrics
        public ActionResult Index(string search = "")
        {
            var metrics = GetMetricItems(search);
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

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult IndexGrid(string search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            return new QueryableResult<MetricDisplayItem>(GetMetricItems(search).AsQueryable());
        }

        private List<MetricDisplayItem> GetMetricItems(string search)
        {
            var snapshots = _applicationMetrics.GetSnapshots();
            IQueryable<KeyValuePair<string, MetricSnapshot>> query = snapshots.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Key.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return query
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
        }
    }
}