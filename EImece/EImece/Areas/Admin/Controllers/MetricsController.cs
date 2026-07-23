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
        public ActionResult Index(string search = "", int page = 1)
        {
            const int pageSize = 50;

            var snapshots = _applicationMetrics.GetSnapshots();

            // Filter by search term if provided
            IQueryable<System.Collections.Generic.KeyValuePair<string, MetricSnapshot>> query = snapshots.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Key.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Convert to display items
            var totalCount = query.Count();
            var metrics = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MetricDisplayItem
                {
                    Name = x.Key,
                    Count = x.Value.Count,
                    ErrorCount = x.Value.ErrorCount,
                    AverageDurationMs = Math.Round(x.Value.AverageDurationMs, 2),
                    P95DurationMs = x.Value.P95DurationMs
                })
                .ToList();

            var viewModel = new MetricsIndexViewModel
            {
                Metrics = metrics,
                SearchTerm = search,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(viewModel);
        }
    }
}
