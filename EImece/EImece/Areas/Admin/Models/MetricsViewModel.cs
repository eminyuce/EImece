using System.Collections.Generic;
using System.Linq;

namespace EImece.Areas.Admin.Models
{
    public class MetricDisplayItem
    {
        public string Name { get; set; }
        public long Count { get; set; }
        public long ErrorCount { get; set; }
        public int SampleWindowSize { get; set; }
        public double AverageDurationMs { get; set; }
        public long P90DurationMs { get; set; }
        public long P95DurationMs { get; set; }
        public long P99DurationMs { get; set; }
    }

    public class MetricsIndexViewModel
    {
        public List<MetricDisplayItem> Metrics { get; set; } = new List<MetricDisplayItem>();
        public string SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }

        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
