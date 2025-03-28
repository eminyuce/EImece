using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.AdminModels
{
    public class DataSetReportViewModel
    {
        public DataSet ReportData { get; set; }
        public string ReportTitle { get; set; }
        public string ReportActionName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsNotEmpty()
        {
            return StartDate!=null && StartDate.HasValue && EndDate != null && EndDate.HasValue;
        }
    }
}
