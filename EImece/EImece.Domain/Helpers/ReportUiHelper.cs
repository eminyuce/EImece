using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EImece.Domain.Helpers
{
    public class ReportKpiItem
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public class ReportExportButtonsModel
    {
        public string ReportKey { get; set; }
        public bool HasData { get; set; }
        public IDictionary<string, object> Filters { get; set; }
    }

    public static class ReportUiHelper
    {
        private static readonly CultureInfo TrCulture = CultureInfo.GetCultureInfo("tr-TR");

        private static readonly Dictionary<string, string> ColumnLabels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Coupon", "Kupon Kodu" },
                { "CouponCode", "Kupon Kodu" },
                { "UsageCount", "Kullanım Sayısı" },
                { "TotalDiscount", "Toplam İndirim" },
                { "OrderCount", "Sipariş Sayısı" },
                { "TotalAmount", "Toplam Tutar" },
                { "TotalRevenue", "Toplam Gelir" },
                { "AverageOrderValue", "Ortalama Sipariş Tutarı" },
                { "Currency", "Para Birimi" },
                { "City", "Şehir" },
                { "Region", "Bölge" },
                { "CardType", "Kart Tipi" },
                { "CardAssociation", "Kart Markası" },
                { "Installment", "Taksit" },
                { "PaymentStatus", "Ödeme Durumu" },
                { "PaymentMethod", "Ödeme Yöntemi" },
                { "ShipmentCompanyName", "Kargo Firması" },
                { "TotalCargoCost", "Toplam Kargo Ücreti" },
                { "OrderNumber", "Sipariş No" },
                { "CreatedDate", "Tarih" },
                { "OrderDate", "Sipariş Tarihi" },
                { "StartDate", "Başlangıç Tarihi" },
                { "EndDate", "Bitiş Tarihi" },
                { "UserId", "Kullanıcı" },
                { "PaidPrice", "Ödenen Tutar" },
                { "Amount", "Tutar" },
                { "FraudStatus", "Sahtekarlık Durumu" },
                { "ErrorMessage", "Hata Mesajı" },
                { "ProductName", "Ürün Adı" },
                { "ProductCode", "Ürün Kodu" },
                { "ProductId", "Ürün Id" },
                { "Quantity", "Adet" },
                { "Stock", "Stok" },
                { "StockQuantity", "Stok Adedi" },
                { "Price", "Fiyat" },
                { "MinPrice", "Min. Fiyat" },
                { "MaxPrice", "Max. Fiyat" },
                { "AvgPrice", "Ort. Fiyat" },
                { "CategoryName", "Kategori" },
                { "ProductCategoryId", "Kategori Id" },
                { "CategoryId", "Kategori Id" },
                { "BrandName", "Marka" },
                { "IsActive", "Aktif" },
                { "UnitPrice", "Birim Fiyat" },
                { "Discount", "İndirim" },
                { "DiscountAmount", "İndirim Tutarı" },
                { "Revenue", "Gelir" },
                { "SalesCount", "Satış Adedi" },
                { "TotalSales", "Toplam Satış" },
                { "TotalQuantity", "Toplam Adet" },
                { "RowCount", "Kayıt Sayısı" }
            };

        private static readonly string[] KpiColumnPriority =
        {
            "OrderCount",
            "TotalRevenue",
            "TotalAmount",
            "TotalDiscount",
            "UsageCount",
            "TotalCargoCost",
            "TotalQuantity",
            "SalesCount",
            "Quantity",
            "PaidPrice",
            "Amount",
            "Revenue",
            "TotalSales"
        };

        private static readonly HashSet<string> MoneyColumnHints =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Amount", "Revenue", "Price", "Discount", "Cost", "Paid",
                "Total", "Value", "Cargo", "Fee", "Tax"
            };

        private static readonly HashSet<string> CountColumnHints =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Count", "Quantity", "Qty", "Stock", "Installment"
            };

        public static string GetDisplayColumnName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return string.Empty;
            }

            string key = columnName.Trim();
            string label;
            if (ColumnLabels.TryGetValue(key, out label))
            {
                return label;
            }

            return SplitPascalCase(key);
        }

        public static string FormatCell(object value, string columnName, Type dataType)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            Type type = dataType ?? value.GetType();
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(DateTime) || value is DateTime)
            {
                return ((DateTime)value).ToString("dd.MM.yyyy", TrCulture);
            }

            if (type == typeof(DateTimeOffset) || value is DateTimeOffset)
            {
                return ((DateTimeOffset)value).ToString("dd.MM.yyyy", TrCulture);
            }

            if (type == typeof(bool) || value is bool)
            {
                return (bool)value ? "Evet" : "Hayır";
            }

            if (IsNumericType(type))
            {
                decimal number;
                if (!decimal.TryParse(System.Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out number))
                {
                    return System.Convert.ToString(value, TrCulture) ?? string.Empty;
                }

                if (IsCountColumn(columnName, type))
                {
                    return number.ToString("N0", TrCulture);
                }

                if (IsMoneyColumn(columnName) || type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                {
                    return number.ToString("N2", TrCulture);
                }

                if (type == typeof(byte) || type == typeof(short) || type == typeof(int) || type == typeof(long)
                    || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong))
                {
                    return number.ToString("N0", TrCulture);
                }

                return number.ToString("N2", TrCulture);
            }

            DateTime parsedDate;
            if (!string.IsNullOrWhiteSpace(columnName)
                && columnName.IndexOf("Date", StringComparison.OrdinalIgnoreCase) >= 0
                && DateTime.TryParse(System.Convert.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return parsedDate.ToString("dd.MM.yyyy", TrCulture);
            }

            return System.Convert.ToString(value, TrCulture) ?? string.Empty;
        }

        public static bool IsRightAlignedColumn(string columnName, Type dataType)
        {
            Type type = dataType != null ? (Nullable.GetUnderlyingType(dataType) ?? dataType) : null;
            if (type != null && IsNumericType(type))
            {
                return true;
            }

            return IsMoneyColumn(columnName) || IsCountColumn(columnName, type);
        }

        public static List<ReportKpiItem> TryBuildKpis(DataTable dt)
        {
            var result = new List<ReportKpiItem>();
            if (dt == null || dt.Rows.Count == 0 || dt.Columns.Count == 0)
            {
                return result;
            }

            foreach (string columnName in KpiColumnPriority)
            {
                if (!dt.Columns.Contains(columnName))
                {
                    continue;
                }

                DataColumn column = dt.Columns[columnName];
                decimal? sum = TrySumColumn(dt, column);
                if (!sum.HasValue)
                {
                    continue;
                }

                result.Add(new ReportKpiItem
                {
                    Label = GetDisplayColumnName(columnName),
                    Value = FormatCell(sum.Value, columnName, typeof(decimal))
                });

                if (result.Count >= 4)
                {
                    break;
                }
            }

            if (result.Count == 0)
            {
                result.Add(new ReportKpiItem
                {
                    Label = "Kayıt Sayısı",
                    Value = dt.Rows.Count.ToString("N0", TrCulture)
                });
            }

            return result;
        }

        public static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string withSpaces = Regex.Replace(value.Replace('_', ' '), "([a-z0-9])([A-Z])", "$1 $2");
            withSpaces = Regex.Replace(withSpaces, "([A-Z]+)([A-Z][a-z])", "$1 $2");
            return withSpaces.Trim();
        }

        private static decimal? TrySumColumn(DataTable dt, DataColumn column)
        {
            decimal sum = 0m;
            bool hasValue = false;

            foreach (DataRow row in dt.Rows)
            {
                object raw = row[column];
                if (raw == null || raw == DBNull.Value)
                {
                    continue;
                }

                decimal number;
                if (raw is decimal || raw is double || raw is float || raw is int || raw is long
                    || raw is short || raw is byte || raw is uint || raw is ulong || raw is ushort)
                {
                    number = System.Convert.ToDecimal(raw, CultureInfo.InvariantCulture);
                }
                else if (!decimal.TryParse(System.Convert.ToString(raw, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out number))
                {
                    return null;
                }

                sum += number;
                hasValue = true;
            }

            return hasValue ? sum : (decimal?)null;
        }

        private static bool IsNumericType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal);
        }

        private static bool IsMoneyColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            if (columnName.Equals("OrderCount", StringComparison.OrdinalIgnoreCase)
                || columnName.Equals("UsageCount", StringComparison.OrdinalIgnoreCase)
                || columnName.Equals("SalesCount", StringComparison.OrdinalIgnoreCase)
                || columnName.Equals("Quantity", StringComparison.OrdinalIgnoreCase)
                || columnName.Equals("TotalQuantity", StringComparison.OrdinalIgnoreCase)
                || columnName.Equals("Stock", StringComparison.OrdinalIgnoreCase)
                || columnName.Equals("StockQuantity", StringComparison.OrdinalIgnoreCase)
                || columnName.Equals("Installment", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return MoneyColumnHints.Any(hint =>
                columnName.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool IsCountColumn(string columnName, Type dataType)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return dataType != null && (
                    dataType == typeof(byte) || dataType == typeof(short) || dataType == typeof(int)
                    || dataType == typeof(long) || dataType == typeof(ushort) || dataType == typeof(uint)
                    || dataType == typeof(ulong));
            }

            return CountColumnHints.Any(hint =>
                columnName.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
