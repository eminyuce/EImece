using System;
using System.Net;
using System.Xml.Linq;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Normalizes product-spec checkbox values for storage and storefront display.
    /// HTML checkboxes post "on" by default; we store true/false and show Evet/Hayır.
    /// </summary>
    public static class ProductSpecificationValueHelper
    {
        public const string TrueStorageValue = "true";
        public const string FalseStorageValue = "false";

        public static bool IsCheckboxField(XElement field)
        {
            return field != null
                && field.Name.LocalName.Equals("checkbox", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTruthyCheckboxValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var v = value.Trim();
            return v.Equals("on", StringComparison.OrdinalIgnoreCase)
                || v.Equals("true", StringComparison.OrdinalIgnoreCase)
                || v.Equals("1", StringComparison.OrdinalIgnoreCase)
                || v.Equals("yes", StringComparison.OrdinalIgnoreCase)
                || v.Equals("evet", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Canonical DB value written from the admin product-specs form.
        /// </summary>
        public static string NormalizeCheckboxStorageValue(string formValue)
        {
            return IsTruthyCheckboxValue(formValue) ? TrueStorageValue : FalseStorageValue;
        }

        /// <summary>
        /// Storefront / customer-facing label (Turkish site).
        /// </summary>
        public static string FormatCheckboxDisplayValue(string storedValue)
        {
            return IsTruthyCheckboxValue(storedValue) ? "Evet" : "Hayır";
        }

        public static string FormatSpecDisplayValue(XElement field, string storedValue)
        {
            if (IsCheckboxField(field))
            {
                return FormatCheckboxDisplayValue(storedValue);
            }

            if (ProductSpecificationFieldHelper.IsDateTimeField(field))
            {
                return ProductSpecificationFieldHelper.FormatDateTimeDisplay(
                    storedValue,
                    ProductSpecificationFieldHelper.IncludeTime(field));
            }

            if (ProductSpecificationFieldHelper.IsMultiSelectField(field))
            {
                var valuesAttr = field.Attribute("values");
                return ProductSpecificationFieldHelper.FormatMultiSelectDisplay(
                    storedValue,
                    valuesAttr != null ? valuesAttr.Value : null);
            }

            var raw = storedValue == null ? string.Empty : storedValue.Trim();
            if (ProductSpecificationFieldHelper.IsTextAreaField(field)
                && !ProductSpecificationFieldHelper.AllowsHtml(field))
            {
                return WebUtility.HtmlEncode(raw);
            }

            return raw;
        }
    }
}
