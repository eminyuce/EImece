using EImece.Domain.Caching;
using System;
using System.Collections.Generic;

namespace EImece.Areas.Admin.Models
{
    public class CacheAdminViewModel
    {
        public CacheMetricsSnapshot Metrics { get; set; } = new CacheMetricsSnapshot();
        public CacheOverviewSnapshot Overview { get; set; } = new CacheOverviewSnapshot();
        public IReadOnlyList<CacheEntrySnapshot> Entries { get; set; } = Array.Empty<CacheEntrySnapshot>();
        public IReadOnlyList<string> Categories { get; set; } = Array.Empty<string>();
        public string Search { get; set; }
        public string Category { get; set; } = "all";
        public string Status { get; set; } = "all";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
        public bool IsCacheActive { get; set; }
        public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    }

    public class CacheLayerCardModel
    {
        public string Title { get; set; }
        public string Lead { get; set; }
        public string TechnicalName { get; set; }
        public string ProfileLabel { get; set; }
        public string Prefix { get; set; }
        public bool ShowDatabaseAvoided { get; set; }
        public CacheLayerSnapshot Layer { get; set; }
    }
}
