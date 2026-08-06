using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace EImece.Domain.Helpers
{
    public class ExcelHelper
    {
        //     <summary>Opens an Excel(xls or xlsx) file and converts it into a DataTable.
        //THE first row must contain the names of those fields.</summary>
        //<param name = "pRutaArchivo" > full path of the file to open.</param>
        //<param name = "pHojaIndex" > number(based on zero) of the sheet that is want to open. 0 is the first sheet.</param>
        public static DataTable Excel_To_DataTable(

            string fileFullPath,
            int sheetIndex
            )
        {
            string pRutaArchivo = fileFullPath; int pHojaIndex = sheetIndex;
            // --------------------------------- //
            /* REFERENCIAS:
             * NPOI.dll
             * NPOI.OOXML.dll
             * NPOI.OpenXml4Net.dll */
            // --------------------------------- //
            /* USING:
             * using NPOI.SS.UserModel;
             * using NPOI.HSSF.UserModel;
             * using NPOI.XSSF.UserModel; */
            // AUTOR: Ing. Jhollman Chacon R. 2015
            // --------------------------------- //
            DataTable Tabla = null;
            try
            {
                if (System.IO.File.Exists(pRutaArchivo))
                {
                    IWorkbook workbook = null;  //IWorkbook determina si es xls o xlsx
                    ISheet worksheet = null;
                    string first_sheet_name = "";

                    using (FileStream FS = new FileStream(pRutaArchivo, FileMode.Open, FileAccess.Read))
                    {
                        workbook = WorkbookFactory.Create(FS);          //Abre tanto XLS como XLSX
                        worksheet = workbook.GetSheetAt(pHojaIndex);    //Obtener Hoja por indice
                        first_sheet_name = worksheet.SheetName;         //Obtener el nombre de la Hoja

                        Tabla = new DataTable(first_sheet_name);
                        Tabla.Rows.Clear();
                        Tabla.Columns.Clear();

                        // Leer Fila por fila desde la primera
                        for (int rowIndex = 0; rowIndex <= worksheet.LastRowNum; rowIndex++)
                        {
                            DataRow NewReg = null;
                            IRow row = worksheet.GetRow(rowIndex);
                            IRow row2 = null;
                            IRow row3 = null;

                            if (rowIndex == 0)
                            {
                                row2 = worksheet.GetRow(rowIndex + 1); //Si es la Primera fila, obtengo tambien la segunda para saber el tipo de datos
                                row3 = worksheet.GetRow(rowIndex + 2); //Y la tercera tambien por las dudas
                            }

                            if (row != null) //null is when the row only contains empty cells
                            {
                                if (rowIndex > 0) NewReg = Tabla.NewRow();

                                int colIndex = 0;
                                //Leer cada Columna de la fila
                                foreach (ICell cell in row.Cells)
                                {
                                    object valorCell = null;
                                    string cellType = "";
                                    string[] cellType2 = new string[2];

                                    if (rowIndex == 0) //Asumo que la primera fila contiene los titlos:
                                    {
                                        for (int i = 0; i < 2; i++)
                                        {
                                            ICell cell2 = null;
                                            if (i == 0) { cell2 = row2.GetCell(cell.ColumnIndex); }
                                            else { cell2 = row3.GetCell(cell.ColumnIndex); }

                                            if (cell2 != null)
                                            {
                                                switch (cell2.CellType)
                                                {
                                                    case CellType.Blank: break;
                                                    case CellType.Boolean: cellType2[i] = "System.Boolean"; break;
                                                    case CellType.String: cellType2[i] = "System.String"; break;
                                                    case CellType.Numeric:
                                                        if (HSSFDateUtil.IsCellDateFormatted(cell2)) { cellType2[i] = "System.DateTime"; }
                                                        else
                                                        {
                                                            cellType2[i] = "System.Double";  //valorCell = cell2.NumericCellValue;
                                                        }
                                                        break;

                                                    case CellType.Formula:
                                                        bool continuar = true;
                                                        switch (cell2.CachedFormulaResultType)
                                                        {
                                                            case CellType.Boolean: cellType2[i] = "System.Boolean"; break;
                                                            case CellType.String: cellType2[i] = "System.String"; break;
                                                            case CellType.Numeric:
                                                                if (HSSFDateUtil.IsCellDateFormatted(cell2)) { cellType2[i] = "System.DateTime"; }
                                                                else
                                                                {
                                                                    try
                                                                    {
                                                                        //DETERMINAR SI ES BOOLEANO
                                                                        if (cell2.CellFormula == "TRUE()") { cellType2[i] = "System.Boolean"; continuar = false; }
                                                                        if (continuar && cell2.CellFormula == "FALSE()") { cellType2[i] = "System.Boolean"; continuar = false; }
                                                                        if (continuar) { cellType2[i] = "System.Double"; continuar = false; }
                                                                    }
                                                                    catch
                                                                    {
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                        break;

                                                    default:
                                                        cellType2[i] = "System.String"; break;
                                                }
                                            }
                                        }

                                        //Resolver las diferencias de Tipos
                                        if (cellType2[0] == cellType2[1]) { cellType = cellType2[0]; }
                                        else
                                        {
                                            if (cellType2[0] == null) cellType = cellType2[1];
                                            if (cellType2[1] == null) cellType = cellType2[0];
                                            if (cellType == "") cellType = "System.String";
                                        }

                                        //Obtener el nombre de la Columna
                                        string colName = "Column_{0}";
                                        try { colName = cell.StringCellValue; }
                                        catch { colName = string.Format(colName, colIndex); }

                                        //Verificar que NO se repita el Nombre de la Columna
                                        foreach (DataColumn col in Tabla.Columns)
                                        {
                                            if (col.ColumnName == colName) colName = string.Format("{0}_{1}", colName, colIndex);
                                        }

                                        //Agregar el campos de la tabla:
                                        DataColumn codigo = new DataColumn(colName, System.Type.GetType(cellType));
                                        Tabla.Columns.Add(codigo); colIndex++;
                                    }
                                    else
                                    {
                                        //Las demas filas son registros:
                                        switch (cell.CellType)
                                        {
                                            case CellType.Blank: valorCell = DBNull.Value; break;
                                            case CellType.Boolean: valorCell = cell.BooleanCellValue; break;
                                            case CellType.String: valorCell = cell.StringCellValue; break;
                                            case CellType.Numeric:
                                                if (HSSFDateUtil.IsCellDateFormatted(cell)) { valorCell = cell.DateCellValue; }
                                                else { valorCell = cell.NumericCellValue; }
                                                break;

                                            case CellType.Formula:
                                                switch (cell.CachedFormulaResultType)
                                                {
                                                    case CellType.Blank: valorCell = DBNull.Value; break;
                                                    case CellType.String: valorCell = cell.StringCellValue; break;
                                                    case CellType.Boolean: valorCell = cell.BooleanCellValue; break;
                                                    case CellType.Numeric:
                                                        if (HSSFDateUtil.IsCellDateFormatted(cell)) { valorCell = cell.DateCellValue; }
                                                        else { valorCell = cell.NumericCellValue; }
                                                        break;
                                                }
                                                break;

                                            default: valorCell = cell.StringCellValue; break;
                                        }
                                        //Agregar el nuevo Registro
                                        if (cell.ColumnIndex <= Tabla.Columns.Count - 1) NewReg[cell.ColumnIndex] = valorCell;
                                    }
                                }
                            }
                            if (rowIndex > 0) Tabla.Rows.Add(NewReg);
                        }
                        Tabla.AcceptChanges();
                    }
                }
                else
                {
                    throw new Exception("ERROR 404: The specified file does NOT exist.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Tabla;
        }

        //        <summary>Converts a DataTable into an Excel(xls or Xlsx) file and saves it to disk.</summary>
        //<param name = "pDatos" > data from the table to save.Uses the name of the table as the name of the worksheet</param>
        //<param name = "pFilePath" > file path where is stored.</param>
        public static void DataTable_To_Excel(DataTable pDatos, string pFilePath)
        {
            try
            {
                if (pDatos != null && pDatos.Rows.Count > 0)
                {
                    IWorkbook workbook = null;
                    ISheet worksheet = null;

                    using (FileStream stream = new FileStream(pFilePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        string Ext = System.IO.Path.GetExtension(pFilePath); //<-Extension del archivo
                        switch (Ext.ToLower())
                        {
                            case ".xls":
                                workbook = new HSSFWorkbook();
                                break;

                            case ".xlsx": workbook = new XSSFWorkbook(); break;
                        }

                        worksheet = workbook.CreateSheet(pDatos.TableName); //<-Usa el nombre de la tabla como nombre de la Hoja

                        //CREAR EN LA PRIMERA FILA LOS TITULOS DE LAS COLUMNAS
                        int iRow = 0;
                        if (pDatos.Columns.Count > 0)
                        {
                            int iCol = 0;
                            IRow fila = worksheet.CreateRow(iRow);
                            foreach (DataColumn columna in pDatos.Columns)
                            {
                                ICell cell = fila.CreateCell(iCol, CellType.String);
                                cell.SetCellValue(columna.ColumnName);
                                iCol++;
                            }
                            iRow++;
                        }

                        //FORMATOS PARA CIERTOS TIPOS DE DATOS
                        ICellStyle _doubleCellStyle = workbook.CreateCellStyle();
                        _doubleCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0.###");

                        ICellStyle _intCellStyle = workbook.CreateCellStyle();
                        _intCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0");

                        ICellStyle _boolCellStyle = workbook.CreateCellStyle();
                        _boolCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("BOOLEAN");

                        ICellStyle _dateCellStyle = workbook.CreateCellStyle();
                        _dateCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("dd-MM-yyyy");

                        ICellStyle _dateTimeCellStyle = workbook.CreateCellStyle();
                        _dateTimeCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("dd-MM-yyyy HH:mm:ss");

                        //AHORA CREAR UNA FILA POR CADA REGISTRO DE LA TABLA
                        foreach (DataRow row in pDatos.Rows)
                        {
                            IRow fila = worksheet.CreateRow(iRow);
                            int iCol = 0;
                            foreach (DataColumn column in pDatos.Columns)
                            {
                                ICell cell = null; //<-Representa la celda actual
                                object cellValue = row[iCol]; //<- El valor actual de la celda

                                switch (column.DataType.ToString())
                                {
                                    case "System.Boolean":
                                        if (!System.Convert.IsDBNull(cellValue))
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Boolean);

                                            if (System.Convert.ToBoolean(cellValue)) { cell.SetCellFormula("TRUE()"); }
                                            else { cell.SetCellFormula("FALSE()"); }

                                            cell.CellStyle = _boolCellStyle;
                                        }
                                        break;

                                    case "System.String":
                                        if (!System.Convert.IsDBNull(cellValue))
                                        {
                                            cell = fila.CreateCell(iCol, CellType.String);
                                            cell.SetCellValue(System.Convert.ToString(cellValue));
                                        }
                                        break;

                                    case "System.Int32":
                                        if (!System.Convert.IsDBNull(cellValue))
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(System.Convert.ToInt32(cellValue));
                                            cell.CellStyle = _intCellStyle;
                                        }
                                        break;

                                    case "System.Int64":
                                        if (!System.Convert.IsDBNull(cellValue))
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(System.Convert.ToInt64(cellValue));
                                            cell.CellStyle = _intCellStyle;
                                        }
                                        break;

                                    case "System.Decimal":
                                        if (!System.Convert.IsDBNull(cellValue))
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(System.Convert.ToDouble(cellValue));
                                            cell.CellStyle = _doubleCellStyle;
                                        }
                                        break;

                                    case "System.Double":
                                        if (!System.Convert.IsDBNull(cellValue))
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(System.Convert.ToDouble(cellValue));
                                            cell.CellStyle = _doubleCellStyle;
                                        }
                                        break;

                                    case "System.DateTime":
                                        if (!System.Convert.IsDBNull(cellValue))
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Convert.ToDateTime(cellValue));

                                            //Si No tiene valor de Hora, usar formato dd-MM-yyyy
                                            DateTime cDate = Convert.ToDateTime(cellValue);
                                            if (cDate != null && cDate.Hour > 0) { cell.CellStyle = _dateTimeCellStyle; }
                                            else { cell.CellStyle = _dateCellStyle; }
                                        }
                                        break;

                                    default:
                                        break;
                                }
                                iCol++;
                            }
                            iRow++;
                        }

                        workbook.Write(stream);
                        stream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public static void DataTableToExcel(DataTable dt, String fileName)
        //{
        //    //Make a new npoi workbook
        //    HSSFWorkbook hssfworkbook = new HSSFWorkbook();
        //    //Here I am making sure that I am giving the file name the right
        //    //extension:
        //    string filename = "";
        //    if (fileName.EndsWith(".xls"))
        //        filename = fileName;
        //    else
        //        filename = fileName + ".xls";
        //    //This starts the dialogue box that allows the user to download the file
        //    System.Web.HttpResponse Response = System.Web.HttpContext.Current.Response;
        //    //Response.ContentType = “application/vnd.ms-excel”;
        //    //Response.AddHeader(“Content-Disposition”, string.Format(“attachment;filename={0}”, filename));
        //    Response.Clear();
        //    //make a new sheet – name it any excel-compliant string you want
        //    var sheet1 = hssfworkbook.CreateSheet("Sheet 1");
        //    //make a header row
        //    var row1 = sheet1.CreateRow(0);
        //    //Puts in headers (these are table row headers, omit if you
        //    //just need a straight data dump
        //    for (int j = 0; j < dt.Columns.Count; j++)
        //    {
        //        var cell = row1.CreateCell(j);
        //        String columnName = dt.Columns[j].ToString();
        //        cell.SetCellValue(columnName);
        //    }

        //    //loops through data

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        var row = sheet1.CreateRow(i + 1);
        //        for (int j = 0; j < dt.Columns.Count; j++)
        //        {
        //            var cell = row.CreateCell(j);
        //            String columnName = dt.Columns[j].ToString();
        //            cell.SetCellValue(dt.Rows[i][columnName].ToString());
        //        }
        //    }

        //    //writing the data to binary from memory

        //    Response.BinaryWrite(WriteToStream(hssfworkbook).GetBuffer());

        //    Response.End();

        //}

        private static MemoryStream WriteToStream(HSSFWorkbook hssfworkbook)
        {
            //Write the stream data of workbook to the root directory
            MemoryStream file = new MemoryStream();
            hssfworkbook.Write(file);
            file.Seek(0, 0);
            return file;
        }

        public static HSSFWorkbook CreateWorkBook(List<DataTable> dtList)
        {
            var workbook = new HSSFWorkbook();
            var styles = CreateExportCellStyles(workbook);
            int i = 1;
            foreach (DataTable dt in dtList)
            {
                if (dt == null)
                {
                    continue;
                }

                String name = String.Format("workbook-{0}", i);
                if (!String.IsNullOrEmpty(dt.TableName))
                {
                    name = dt.TableName;
                }
                else
                {
                    i++;
                }
                var sheet = workbook.CreateSheet(name);
                ExportDataTableToSheet(dt, sheet, styles);
            }
            return workbook;
        }

        private sealed class ExportCellStyles
        {
            public ICellStyle Header { get; set; }
            public ICellStyle Text { get; set; }
            public ICellStyle Numeric { get; set; }
            public ICellStyle Integer { get; set; }
            public ICellStyle Boolean { get; set; }
            public ICellStyle Date { get; set; }
            public ICellStyle DateTime { get; set; }
        }

        private static ExportCellStyles CreateExportCellStyles(HSSFWorkbook workbook)
        {
            var dataFormat = workbook.CreateDataFormat();

            var headerFont = workbook.CreateFont();
            headerFont.FontName = "Calibri";
            headerFont.FontHeightInPoints = 12;
            headerFont.IsBold = true;
            headerFont.Color = HSSFColor.Black.Index;

            var dataFont = workbook.CreateFont();
            dataFont.FontName = "Calibri";
            dataFont.FontHeightInPoints = 11;
            dataFont.IsBold = false;
            dataFont.Color = HSSFColor.Black.Index;

            var headerStyle = workbook.CreateCellStyle();
            headerStyle.SetFont(headerFont);
            headerStyle.FillForegroundColor = HSSFColor.LightYellow.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.Alignment = HorizontalAlignment.Center;
            headerStyle.VerticalAlignment = VerticalAlignment.Center;
            headerStyle.BorderTop = BorderStyle.Thin;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BorderLeft = BorderStyle.Thin;
            headerStyle.BorderRight = BorderStyle.Thin;

            var textStyle = workbook.CreateCellStyle();
            textStyle.SetFont(dataFont);
            textStyle.Alignment = HorizontalAlignment.Left;
            textStyle.VerticalAlignment = VerticalAlignment.Center;

            var numericStyle = workbook.CreateCellStyle();
            numericStyle.SetFont(dataFont);
            numericStyle.Alignment = HorizontalAlignment.Right;
            numericStyle.VerticalAlignment = VerticalAlignment.Center;
            numericStyle.DataFormat = dataFormat.GetFormat("#,##0.###");

            var integerStyle = workbook.CreateCellStyle();
            integerStyle.SetFont(dataFont);
            integerStyle.Alignment = HorizontalAlignment.Right;
            integerStyle.VerticalAlignment = VerticalAlignment.Center;
            integerStyle.DataFormat = dataFormat.GetFormat("#,##0");

            var booleanStyle = workbook.CreateCellStyle();
            booleanStyle.SetFont(dataFont);
            booleanStyle.Alignment = HorizontalAlignment.Left;
            booleanStyle.VerticalAlignment = VerticalAlignment.Center;

            var dateStyle = workbook.CreateCellStyle();
            dateStyle.SetFont(dataFont);
            dateStyle.Alignment = HorizontalAlignment.Left;
            dateStyle.VerticalAlignment = VerticalAlignment.Center;
            dateStyle.DataFormat = dataFormat.GetFormat("dd.MM.yyyy");

            var dateTimeStyle = workbook.CreateCellStyle();
            dateTimeStyle.SetFont(dataFont);
            dateTimeStyle.Alignment = HorizontalAlignment.Left;
            dateTimeStyle.VerticalAlignment = VerticalAlignment.Center;
            dateTimeStyle.DataFormat = dataFormat.GetFormat("dd.MM.yyyy HH:mm:ss");

            return new ExportCellStyles
            {
                Header = headerStyle,
                Text = textStyle,
                Numeric = numericStyle,
                Integer = integerStyle,
                Boolean = booleanStyle,
                Date = dateStyle,
                DateTime = dateTimeStyle
            };
        }

        private static void ExportDataTableToSheet(DataTable dt, ISheet sheet, ExportCellStyles styles)
        {
            if (dt == null || sheet == null || styles == null)
            {
                return;
            }

            int columnCount = dt.Columns.Count;
            if (columnCount == 0)
            {
                return;
            }

            var headerRow = sheet.CreateRow(0);
            for (int j = 0; j < columnCount; j++)
            {
                var cell = headerRow.CreateCell(j, CellType.String);
                cell.SetCellValue(dt.Columns[j].ColumnName);
                cell.CellStyle = styles.Header;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = sheet.CreateRow(i + 1);
                for (int j = 0; j < columnCount; j++)
                {
                    WriteTypedCell(row, j, dt.Rows[i][j], dt.Columns[j], styles);
                }
            }

            int lastRowIndex = Math.Max(dt.Rows.Count, 0);
            sheet.CreateFreezePane(0, 1);
            sheet.SetAutoFilter(new CellRangeAddress(0, lastRowIndex, 0, columnCount - 1));

            for (int j = 0; j < columnCount; j++)
            {
                sheet.AutoSizeColumn(j);
            }
        }

        private static void WriteTypedCell(IRow row, int columnIndex, object cellValue, DataColumn column, ExportCellStyles styles)
        {
            if (cellValue == null || cellValue == DBNull.Value)
            {
                var emptyCell = row.CreateCell(columnIndex, CellType.Blank);
                emptyCell.CellStyle = styles.Text;
                return;
            }

            Type type = column != null ? column.DataType : cellValue.GetType();
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(Boolean) || cellValue is bool)
            {
                var cell = row.CreateCell(columnIndex, CellType.Boolean);
                cell.SetCellValue(System.Convert.ToBoolean(cellValue));
                cell.CellStyle = styles.Boolean;
                return;
            }

            if (type == typeof(DateTime) || cellValue is DateTime)
            {
                var dateValue = System.Convert.ToDateTime(cellValue);
                var cell = row.CreateCell(columnIndex, CellType.Numeric);
                cell.SetCellValue(dateValue);
                cell.CellStyle = dateValue.TimeOfDay.TotalSeconds == 0 ? styles.Date : styles.DateTime;
                return;
            }

            if (type == typeof(Byte) || type == typeof(SByte)
                || type == typeof(Int16) || type == typeof(UInt16)
                || type == typeof(Int32) || type == typeof(UInt32)
                || type == typeof(Int64) || type == typeof(UInt64)
                || cellValue is byte || cellValue is sbyte
                || cellValue is short || cellValue is ushort
                || cellValue is int || cellValue is uint
                || cellValue is long || cellValue is ulong)
            {
                var cell = row.CreateCell(columnIndex, CellType.Numeric);
                cell.SetCellValue(System.Convert.ToDouble(cellValue));
                cell.CellStyle = styles.Integer;
                return;
            }

            if (type == typeof(Decimal) || type == typeof(Double) || type == typeof(Single)
                || cellValue is decimal || cellValue is double || cellValue is float)
            {
                var cell = row.CreateCell(columnIndex, CellType.Numeric);
                cell.SetCellValue(System.Convert.ToDouble(cellValue));
                cell.CellStyle = styles.Numeric;
                return;
            }

            var textCell = row.CreateCell(columnIndex, CellType.String);
            textCell.SetCellValue(System.Convert.ToString(cellValue) ?? string.Empty);
            textCell.CellStyle = styles.Text;
        }

        public static byte[] Export(DataTable dt, bool exportColumnHeadings)
        {
            var myExport = new CsvExport();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                myExport.AddRow();
                var row = dt.Rows[i];
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    var column = dt.Columns[j];
                    myExport[column.ColumnName] = row[column];
                }
            }

            return myExport.ExportToBytes();
        }

        /// <summary>
        /// CSV tuned for Turkish Excel: semicolon separator, UTF-8 BOM, tr-TR number/date formatting.
        /// Avoids quoted amounts like "485,00" that break when the delimiter is also a comma.
        /// </summary>
        public static byte[] ExportReportCsv(DataTable dt)
        {
            // ; + sep=; so Excel (tr-TR) opens columns correctly while decimals can keep comma
            var myExport = new CsvExport(";", includeColumnSeparatorDefinitionPreamble: true);
            var culture = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");

            if (dt == null)
            {
                return myExport.ExportToBytes();
            }

            if (dt.Rows.Count == 0)
            {
                // Emit header-only CSV (one empty data row keeps column order in CsvExport)
                myExport.AddRow();
                foreach (DataColumn column in dt.Columns)
                {
                    myExport[ReportUiHelper.GetDisplayColumnName(column.ColumnName)] = string.Empty;
                }
                return myExport.ExportToBytes();
            }

            foreach (DataRow row in dt.Rows)
            {
                myExport.AddRow();
                foreach (DataColumn column in dt.Columns)
                {
                    string header = ReportUiHelper.GetDisplayColumnName(column.ColumnName);
                    myExport[header] = FormatReportCsvValue(row[column], column, culture);
                }
            }

            return myExport.ExportToBytes();
        }

        private static object FormatReportCsvValue(object raw, DataColumn column, System.Globalization.CultureInfo culture)
        {
            if (raw == null || raw == DBNull.Value)
            {
                return string.Empty;
            }

            Type type = column != null ? column.DataType : raw.GetType();
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(DateTime) || raw is DateTime)
            {
                var dtValue = (DateTime)raw;
                if (dtValue.TimeOfDay.TotalSeconds == 0)
                {
                    return dtValue.ToString("dd.MM.yyyy", culture);
                }
                return dtValue.ToString("dd.MM.yyyy HH:mm:ss", culture);
            }

            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float)
                || raw is decimal || raw is double || raw is float)
            {
                decimal number = System.Convert.ToDecimal(raw, System.Globalization.CultureInfo.InvariantCulture);
                string name = column != null ? column.ColumnName : string.Empty;
                if (!string.IsNullOrEmpty(name)
                    && (name.IndexOf("Count", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("Quantity", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("Installment", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return number.ToString("0", culture);
                }
                // Money / totals: always 2 decimal places (tr-TR → 485,00)
                return number.ToString("0.00", culture);
            }

            if (type == typeof(bool) || raw is bool)
            {
                return (bool)raw ? "Evet" : "Hayır";
            }

            return System.Convert.ToString(raw, culture) ?? string.Empty;
        }

        public static byte[] GetExcelByteArrayFromDataTable(DataTable dt)
        {
            var dtList = new List<DataTable>();
            dtList.Add(dt);
            var ms = GetExcelByteArrayFromDataTable(dtList);
            return ms.ToArray();
        }

        public static byte[] GetExcelByteArrayFromDataTable(List<DataTable> dtList)
        {
            var ms = GetExcelMemoryFromDataTable(dtList);
            return ms.ToArray();
        }

        public static MemoryStream GetExcelMemoryFromDataTable(List<DataTable> dtList)
        {
            var workbook = CreateWorkBook(dtList);
            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Seek(0, 0);

            return ms;
        }

        public static HSSFWorkbook GetExcel1()
        {
            var workbook = new HSSFWorkbook();
            var ExampleSheet = workbook.CreateSheet("Example Sheet");
            var rowIndex = 0;
            var row = ExampleSheet.CreateRow(rowIndex);
            row.CreateCell(0).SetCellValue("Example in first cell(0,0)");
            rowIndex++;
            return workbook;
        }

        public static MemoryStream GetExcelMemory()
        {
            MemoryStream ms = new MemoryStream();
            var templateWorkbook = GetExcel();
            templateWorkbook.Write(ms);

            return ms;
        }

        public static HSSFWorkbook GetExcel()
        {
            HSSFWorkbook workbook = new HSSFWorkbook();

            ISheet sheet = workbook.CreateSheet("Sheet1");
            //increase the width of Column A
            sheet.SetColumnWidth(0, 5000);
            //create the format instance
            IDataFormat format = workbook.CreateDataFormat();

            // Create a row and put some cells in it. Rows are 0 based.
            ICell cell = sheet.CreateRow(0).CreateCell(0);
            //number format with 2 digits after the decimal point - "1.20"
            SetValueAndFormat(workbook, cell, 1.2, HSSFDataFormat.GetBuiltinFormat("0.00"));

            //RMB currency format with comma    -   "¥20,000"
            ICell cell2 = sheet.CreateRow(1).CreateCell(0);
            SetValueAndFormat(workbook, cell2, 20000, format.GetFormat("¥#,##0"));

            //scentific number format   -   "3.15E+00"
            ICell cell3 = sheet.CreateRow(2).CreateCell(0);
            SetValueAndFormat(workbook, cell3, 3.151234, format.GetFormat("0.00E+00"));

            //percent format, 2 digits after the decimal point    -  "99.33%"
            ICell cell4 = sheet.CreateRow(3).CreateCell(0);
            SetValueAndFormat(workbook, cell4, 0.99333, format.GetFormat("0.00%"));

            //phone number format - "021-65881234"
            ICell cell5 = sheet.CreateRow(4).CreateCell(0);
            SetValueAndFormat(workbook, cell5, 02165881234, format.GetFormat("000-00000000"));

            //Chinese capitalized character number - 壹贰叁 元
            ICell cell6 = sheet.CreateRow(5).CreateCell(0);
            SetValueAndFormat(workbook, cell6, 123, format.GetFormat("[DbNum2][$-804]0 元"));

            //Chinese date string
            ICell cell7 = sheet.CreateRow(6).CreateCell(0);
            SetValueAndFormat(workbook, cell7, new DateTime(2004, 5, 6), format.GetFormat("yyyy年m月d日"));
            cell7.SetCellValue(new DateTime(2004, 5, 6));

            //Chinese date string
            ICell cell8 = sheet.CreateRow(7).CreateCell(0);
            SetValueAndFormat(workbook, cell8, new DateTime(2005, 11, 6), format.GetFormat("yyyy年m月d日"));

            //formula value with datetime style
            ICell cell9 = sheet.CreateRow(8).CreateCell(0);
            cell9.CellFormula = "DateValue(\"2005-11-11\")+TIMEVALUE(\"11:11:11\")";
            ICellStyle cellStyle9 = workbook.CreateCellStyle();
            cellStyle9.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy h:mm");
            cell9.CellStyle = cellStyle9;

            //display current time
            ICell cell10 = sheet.CreateRow(9).CreateCell(0);
            SetValueAndFormat(workbook, cell10, DateTime.Now, format.GetFormat("[$-409]h:mm:ss AM/PM;@"));

            ISheet sheet2 = workbook.CreateSheet("Sheet2");
            //increase the width of Column A
            sheet2.SetColumnWidth(0, 5000);

            // Create a row and put some cells in it. Rows are 0 based.
            ICell cell21 = sheet2.CreateRow(0).CreateCell(0);
            //number format with 2 digits after the decimal point - "1.20"
            SetValueAndFormat(workbook, cell21, 1.2, HSSFDataFormat.GetBuiltinFormat("0.00"));

            return workbook;
        }

        private static void SetValueAndFormat(IWorkbook workbook, ICell cell, int value, short formatId)
        {
            cell.SetCellValue(value);
            ICellStyle cellStyle = workbook.CreateCellStyle();
            cellStyle.DataFormat = formatId;
            cell.CellStyle = cellStyle;
        }

        private static void SetValueAndFormat(IWorkbook workbook, ICell cell, double value, short formatId)
        {
            cell.SetCellValue(value);
            ICellStyle cellStyle = workbook.CreateCellStyle();
            cellStyle.DataFormat = formatId;
            cell.CellStyle = cellStyle;
        }

        private static void SetValueAndFormat(IWorkbook workbook, ICell cell, DateTime value, short formatId)
        {
            //set value for the cell
            if (value != null)
                cell.SetCellValue(value);

            ICellStyle cellStyle = workbook.CreateCellStyle();
            cellStyle.DataFormat = formatId;
            cell.CellStyle = cellStyle;
        }

        public static List<String> GetWorkSheets(String filePath)
        {
            string strConn = GetConnectionString(filePath);
            // string strConn = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=\"Excel 12.0 Xml;HDR=YES\";", filePath);
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            var schemaTable = conn.GetOleDbSchemaTable(
               OleDbSchemaGuid.Tables,
               new object[] { null, null, null, "TABLE" });
            var list = new List<String>();

            foreach (DataRow dr in schemaTable.Rows)
            {
                list.Add(dr["table_name"].ToString());
            }

            return list;
        }

        public static DataSet GetDS(String filePath, String selectedWorkSheet)
        {
            string strConn = GetConnectionString(filePath);
            // string strConn = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=\"Excel 12.0 Xml;HDR=YES\";", filePath);
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;
            strExcel = String.Format("select * from [{0}]", selectedWorkSheet);
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds);
            return ds;
        }

        public static DataTable ExcelToDataTable(string pathName, string sheetName = "")
        {
            DataTable tbContainer = new DataTable();
            String strConn = GetConnectionString(pathName);
            if (string.IsNullOrEmpty(sheetName)) { sheetName = "Sheet1"; }
            OleDbConnection cnnxls = new OleDbConnection(strConn);
            OleDbDataAdapter oda = new OleDbDataAdapter(string.Format("select * from [{0}]", sheetName), cnnxls);
            DataSet ds = new DataSet();
            oda.Fill(tbContainer);
            return tbContainer;
        }

        private static string GetConnectionString(string pathName)
        {
            string strConn = string.Empty;
            FileInfo file = new FileInfo(pathName);
            if (!file.Exists) { throw new Exception("Error, file doesn't exists!"); }
            string extension = file.Extension;
            switch (extension)
            {
                case ".xls":
                    strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                    break;

                case ".xlsx":
                    strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";
                    break;

                default:
                    strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                    break;
            }

            return strConn;
        }

        public static void ExecuteSqlCommand(string connectionString, String sql)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand c = new SqlCommand(sql, connection))
                {
                    c.CommandType = CommandType.Text;
                    c.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public static void ImportCVSFileToDatabase(String connectionString,
            string csvFilePath,
            string primaryKeyColumnName = "", string tableName = "")
        {
            var csvData = File.ReadAllText(csvFilePath);
            var reader = new CSVReader(csvData);
            DataTable dt = reader.CreateDataTable(true);
            if (!String.IsNullOrEmpty(primaryKeyColumnName))
            {
                dt.PrimaryKey = new DataColumn[] { dt.Columns[primaryKeyColumnName] };
            }
            dt.TableName = tableName;
            var sqlCreate = DataTableHelper.GetCreateTableSql(dt);
            Console.WriteLine(sqlCreate);
            ExcelHelper.ExecuteSqlCommand(connectionString, sqlCreate);
            ExcelHelper.SaveTable(dt, connectionString);
        }

        public static void SaveTable(DataTable myDataTable, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.BulkCopyTimeout = 1000000;
                    bulkCopy.BatchSize = myDataTable.Rows.Count + 1;

                    foreach (DataColumn c in myDataTable.Columns)
                        bulkCopy.ColumnMappings.Add(c.ColumnName, c.ColumnName);

                    bulkCopy.DestinationTableName = myDataTable.TableName;
                    try
                    {
                        bulkCopy.WriteToServer(myDataTable);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}