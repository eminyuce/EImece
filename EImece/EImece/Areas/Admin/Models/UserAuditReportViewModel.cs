using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Models
{
    public class UserAuditReportViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserId { get; set; }
        public string TableName { get; set; }
        public string ActionType { get; set; }
        public string ActiveTab { get; set; } = "summary";

        public IList<SelectListItem> AvailableUsers { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailableTables { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailableActionTypes { get; set; } = new List<SelectListItem>();

        public DataTable UserSummaryData { get; set; } = new DataTable();
        public DataTable MonthlyBreakdownData { get; set; } = new DataTable();
        public DataTable DetailedRecordsData { get; set; } = new DataTable();

        public int TotalUsersCount { get; set; }
        public int TotalCreatedCount { get; set; }
        public int TotalUpdatedCount { get; set; }
        public int TotalActivityCount { get; set; }
    }
}
