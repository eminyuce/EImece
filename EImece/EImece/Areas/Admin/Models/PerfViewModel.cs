using EImece.Domain.Observability.Metrics;
using System.Collections.Generic;

namespace EImece.Areas.Admin.Models
{
    public class PerfIndexViewModel
    {
        public List<PerfStatSnapshot> Stats { get; set; } = new List<PerfStatSnapshot>();
        public string SearchTerm { get; set; }
        public string SelectedType { get; set; } = "all";
        public int TotalCount => Stats?.Count ?? 0;
    }
}
