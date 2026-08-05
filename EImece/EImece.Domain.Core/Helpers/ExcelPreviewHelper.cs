using System.Data;
using ClosedXML.Excel;

namespace EImece.Domain.Core.Helpers;

public static class ExcelPreviewHelper
{
    /// <summary>Reads the first worksheet of an Excel file into a DataTable (header row = column names).</summary>
    public static DataTable ReadFirstSheet(string filePath, int maxRows = 500)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet(1);
        var range = worksheet.RangeUsed();
        var table = new DataTable(Path.GetFileNameWithoutExtension(filePath));

        if (range is null)
        {
            return table;
        }

        var firstRow = range.FirstRow();
        var lastRow = range.LastRow().RowNumber();
        var lastCol = range.LastColumn().ColumnNumber();
        var headerRow = firstRow.RowNumber();
        var dataStart = headerRow + 1;
        var dataEnd = Math.Min(lastRow, headerRow + maxRows);

        for (var col = 1; col <= lastCol; col++)
        {
            var header = firstRow.Cell(col).GetString().Trim();
            if (string.IsNullOrEmpty(header))
            {
                header = $"Column{col}";
            }

            var unique = header;
            var suffix = 1;
            while (table.Columns.Contains(unique))
            {
                unique = $"{header}_{suffix++}";
            }

            table.Columns.Add(unique);
        }

        for (var row = dataStart; row <= dataEnd; row++)
        {
            var dataRow = table.NewRow();
            for (var col = 1; col <= lastCol; col++)
            {
                dataRow[col - 1] = worksheet.Cell(row, col).GetFormattedString();
            }

            table.Rows.Add(dataRow);
        }

        return table;
    }
}
