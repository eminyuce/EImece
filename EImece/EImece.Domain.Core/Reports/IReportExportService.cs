using System.Data;

namespace EImece.Domain.Core.Reports;

public interface IReportExportService
{
    byte[] ToExcel(DataTable table, string sheetName = "Report");
    byte[] ToExcel(DataSet dataSet, string? workbookName = null);
    byte[] ToCsv(DataTable table);
    byte[] ToCsv(DataSet dataSet);
}
