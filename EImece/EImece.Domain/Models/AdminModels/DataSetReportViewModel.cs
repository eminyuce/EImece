using System;
using System.Data;

namespace EImece.Domain.Models.AdminModels
{
    public class DataSetReportViewModel
    {
        public DataSet ReportData { get; set; }
        public string ReportTitle { get; set; }
        public string ReportActionName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ProductId { get; set; }
        public string ProductCode { get; set; }
        public int? Lang { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? ProductCategoryId { get; set; }
        public string State { get; set; }
        public bool? IsCampaign { get; set; }
        public bool? MainPage { get; set; }
        public bool? IsActive { get; set; }

        public bool IsNotEmpty()
        {
            return StartDate.HasValue && EndDate.HasValue;
        }
    }
}
