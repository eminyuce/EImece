using System.Data;
using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;

namespace EImece.Domain.Core.Reports;

public sealed class ReportExportService : IReportExportService
{
    public byte[] ToExcel(DataTable table, string sheetName = "Report")
    {
        using var workbook = new XLWorkbook();
        var safeName = SanitizeSheetName(sheetName);
        var worksheet = workbook.Worksheets.Add(table, safeName);
        worksheet.Columns().AdjustToContents(1, 40);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ToExcel(DataSet dataSet, string? workbookName = null)
    {
        using var workbook = new XLWorkbook();
        if (dataSet.Tables.Count == 0)
        {
            workbook.Worksheets.Add("Empty");
        }
        else
        {
            for (var i = 0; i < dataSet.Tables.Count; i++)
            {
                var table = dataSet.Tables[i];
                var name = string.IsNullOrWhiteSpace(table.TableName) || table.TableName.StartsWith("Table", StringComparison.Ordinal)
                    ? $"Sheet{i + 1}"
                    : table.TableName;
                workbook.Worksheets.Add(table, SanitizeSheetName(name));
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ToCsv(DataTable table)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        foreach (DataColumn column in table.Columns)
        {
            csv.WriteField(column.ColumnName);
        }

        csv.NextRecord();
        foreach (DataRow row in table.Rows)
        {
            foreach (DataColumn column in table.Columns)
            {
                csv.WriteField(row[column]?.ToString() ?? string.Empty);
            }

            csv.NextRecord();
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(writer.ToString())).ToArray();
    }

    public byte[] ToCsv(DataSet dataSet)
    {
        if (dataSet.Tables.Count == 0)
        {
            return Encoding.UTF8.GetBytes(string.Empty);
        }

        return ToCsv(dataSet.Tables[0]);
    }

    private static string SanitizeSheetName(string name)
    {
        var cleaned = new string(name.Where(ch => ch != ':' && ch != '\\' && ch != '/' && ch != '?' && ch != '*' && ch != '[' && ch != ']').ToArray());
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            cleaned = "Report";
        }

        return cleaned.Length > 31 ? cleaned[..31] : cleaned;
    }
}
