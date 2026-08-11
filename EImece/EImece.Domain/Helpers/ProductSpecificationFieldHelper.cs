using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Shared helpers for product-spec template field types (options, datetime attrs).
    /// </summary>
    public static class ProductSpecificationFieldHelper
    {
        public static bool IsFieldType(XElement field, string typeName)
        {
            return field != null
                && field.Name.LocalName.Equals(typeName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTextAreaField(XElement field)
        {
            return IsFieldType(field, "textarea");
        }

        public static bool IsRadioField(XElement field)
        {
            return IsFieldType(field, "radio");
        }

        /// <summary>
        /// Multi-select checkboxes: multiselect / checkboxes / multicheckbox.
        /// Stored and displayed as comma-separated values.
        /// </summary>
        public static bool IsMultiSelectField(XElement field)
        {
            return IsFieldType(field, "multiselect")
                || IsFieldType(field, "checkboxes")
                || IsFieldType(field, "multicheckbox");
        }

        public static bool IsDateTimeField(XElement field)
        {
            return IsFieldType(field, "datetime") || IsFieldType(field, "date");
        }

        public static IList<string> ParseCsvValues(string csv)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(csv))
            {
                return result;
            }

            foreach (var part in csv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var v = part.Trim();
                if (v.Length > 0)
                {
                    result.Add(v);
                }
            }

            return result;
        }

        public static bool IsValueSelected(string selectedCsv, string optionValue)
        {
            if (string.IsNullOrWhiteSpace(optionValue))
            {
                return false;
            }

            var selected = ParseCsvValues(selectedCsv);
            return selected.Any(s => s.Equals(optionValue.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Canonical storage: "A, B, C" (trimmed, comma+space).
        /// </summary>
        public static string NormalizeMultiSelectStorageValue(IEnumerable<string> values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            var parts = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return string.Join(", ", parts);
        }

        public static string NormalizeMultiSelectStorageValue(string formValue)
        {
            return NormalizeMultiSelectStorageValue(ParseCsvValues(formValue));
        }

        public static string FormatMultiSelectDisplay(string storedValue, string valuesAttr)
        {
            var selected = ParseCsvValues(storedValue);
            if (!selected.Any())
            {
                return string.Empty;
            }

            var options = ResolveOptions(valuesAttr, null);
            if (options == null || !options.Any())
            {
                return string.Join(", ", selected);
            }

            var labels = new List<string>();
            foreach (var sel in selected)
            {
                var match = options.FirstOrDefault(o =>
                    (o.Value ?? "").Equals(sel, StringComparison.OrdinalIgnoreCase)
                    || (o.Text ?? "").Equals(sel, StringComparison.OrdinalIgnoreCase));
                labels.Add(match != null && !string.IsNullOrEmpty(match.Text) ? match.Text : sel);
            }

            return string.Join(", ", labels);
        }

        /// <summary>
        /// datetime time="false" or element name "date" → date-only picker.
        /// </summary>
        public static bool IncludeTime(XElement field)
        {
            if (field == null)
            {
                return true;
            }

            if (field.Name.LocalName.Equals("date", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var timeAttr = field.Attribute("time");
            if (timeAttr == null || string.IsNullOrWhiteSpace(timeAttr.Value))
            {
                return true;
            }

            var v = timeAttr.Value.Trim();
            if (v.Equals("false", StringComparison.OrdinalIgnoreCase)
                || v.Equals("0", StringComparison.OrdinalIgnoreCase)
                || v.Equals("no", StringComparison.OrdinalIgnoreCase)
                || v.Equals("hayir", StringComparison.OrdinalIgnoreCase)
                || v.Equals("hayır", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static bool AllowsHtml(XElement field)
        {
            if (!IsTextAreaField(field))
            {
                return false;
            }

            var htmlAttr = field.Attribute("html");
            if (htmlAttr == null || string.IsNullOrWhiteSpace(htmlAttr.Value))
            {
                return true; // textarea defaults to HTML-capable
            }

            var v = htmlAttr.Value.Trim();
            return !(v.Equals("false", StringComparison.OrdinalIgnoreCase)
                || v.Equals("0", StringComparison.OrdinalIgnoreCase)
                || v.Equals("no", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Resolve dropdown/radio options from values="ListName" or values="A,B,C".
        /// </summary>
        public static IList<SelectListItem> ResolveOptions(string valuesAttr, string selectedValue)
        {
            var items = new List<SelectListItem>();
            if (string.IsNullOrWhiteSpace(valuesAttr))
            {
                return items;
            }

            selectedValue = selectedValue == null ? "" : selectedValue.Trim();

            if (valuesAttr.IndexOf(',') >= 0)
            {
                foreach (var part in valuesAttr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var v = part.Trim();
                    if (v.Length == 0)
                    {
                        continue;
                    }
                    items.Add(new SelectListItem
                    {
                        Text = v,
                        Value = v,
                        Selected = selectedValue.Equals(v, StringComparison.OrdinalIgnoreCase)
                    });
                }
                return items;
            }

            try
            {
                var listService = DependencyResolver.Current.GetService<IListService>();
                var list = listService != null ? listService.GetListByName(valuesAttr.Trim()) : null;
                if (list != null && list.ListItems != null)
                {
                    foreach (var li in list.ListItems.OrderBy(i => i.Position))
                    {
                        var val = (li.Value ?? li.Name ?? "").Trim();
                        var text = (li.Name ?? li.Value ?? "").Trim();
                        if (string.IsNullOrEmpty(val) && string.IsNullOrEmpty(text))
                        {
                            continue;
                        }
                        items.Add(new SelectListItem
                        {
                            Text = string.IsNullOrEmpty(text) ? val : text,
                            Value = string.IsNullOrEmpty(val) ? text : val,
                            Selected = selectedValue.Equals(val, StringComparison.OrdinalIgnoreCase)
                                || selectedValue.Equals(text, StringComparison.OrdinalIgnoreCase)
                        });
                    }
                    return items;
                }
            }
            catch
            {
                // fall through to single inline option
            }

            var single = valuesAttr.Trim();
            items.Add(new SelectListItem
            {
                Text = single,
                Value = single,
                Selected = selectedValue.Equals(single, StringComparison.OrdinalIgnoreCase)
            });
            return items;
        }

        public static string FormatDateTimeDisplay(string storedValue, bool includeTime)
        {
            if (string.IsNullOrWhiteSpace(storedValue))
            {
                return string.Empty;
            }

            var raw = storedValue.Trim();
            DateTime dt;
            string[] formats =
            {
                "yyyy-MM-dd'T'HH:mm",
                "yyyy-MM-dd'T'HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "dd.MM.yyyy HH:mm",
                "dd.MM.yyyy"
            };

            if (DateTime.TryParseExact(raw, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt)
                || DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt)
                || DateTime.TryParse(raw, new CultureInfo("tr-TR"), DateTimeStyles.AssumeLocal, out dt))
            {
                return includeTime
                    ? dt.ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("tr-TR"))
                    : dt.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("tr-TR"));
            }

            return raw;
        }

        /// <summary>
        /// Normalize posted datetime value for storage (keep ISO-ish form).
        /// </summary>
        public static string NormalizeDateTimeStorageValue(string formValue, bool includeTime)
        {
            if (string.IsNullOrWhiteSpace(formValue))
            {
                return string.Empty;
            }

            var raw = formValue.Trim();
            DateTime dt;
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt)
                || DateTime.TryParse(raw, new CultureInfo("tr-TR"), DateTimeStyles.AssumeLocal, out dt))
            {
                return includeTime
                    ? dt.ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture)
                    : dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return raw;
        }
    }
}
