using EImece.Domain;
using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EImece.Web.Helpers.HtmlHelpers
{
    /// <summary>
    /// Shared admin grid cell helpers. Status toggles keep the legacy span contract
    /// (name, gridkey-id, grid-data-value, gridActiveIcon/gridNotActiveIcon) so
    /// adminEimece.js bulk actions and changeStateSuccess continue to work.
    /// </summary>
    public static class AdminGridHtmlHelpers
    {
        /// <summary>
        /// Field key passed to /admin/Ajax/Change{Grid}OrderingOrState as "checkbox".
        /// Use: State, ImageState, MainPage, IsCampaign.
        /// </summary>
        public static IHtmlString GridStatusToggle(
            this HtmlHelper html,
            int entityId,
            bool isOn,
            string fieldKey,
            string label,
            string shortLabel = null)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }

            if (string.IsNullOrWhiteSpace(fieldKey))
            {
                throw new ArgumentException("fieldKey is required", nameof(fieldKey));
            }

            var spanName = "span" + fieldKey;
            var iconClass = isOn ? Constants.OkStyle : Constants.CancelStyle;
            // Constants.*Style already includes class='...'
            var displayLabel = string.IsNullOrWhiteSpace(shortLabel) ? label : shortLabel;
            var pressed = isOn ? "true" : "false";
            var onOff = isOn ? "is-on" : "is-off";

            var sb = new StringBuilder(384);
            sb.Append("<button type=\"button\" class=\"eg-status-toggle ")
              .Append(onOff)
              .Append("\" data-eg-status-toggle=\"1\" data-eg-status-field=\"")
              .Append(HttpUtility.HtmlAttributeEncode(fieldKey))
              .Append("\" title=\"")
              .Append(HttpUtility.HtmlAttributeEncode(label))
              .Append("\" aria-label=\"")
              .Append(HttpUtility.HtmlAttributeEncode(label))
              .Append("\" aria-pressed=\"")
              .Append(pressed)
              .Append("\">");

            sb.Append("<span class=\"eg-status-switch\" aria-hidden=\"true\">")
              .Append("<span class=\"eg-status-knob\"></span>")
              .Append("</span>");

            // Keep the exact legacy span API for bulk selection + changeStateSuccess.
            sb.Append("<span gridkey-id=\"")
              .Append(entityId)
              .Append("\" grid-data-value=\"")
              .Append(isOn ? "True" : "False")
              .Append("\" ")
              .Append(iconClass.Replace("class='", "class='eg-status-icon "))
              .Append(" name=\"")
              .Append(HttpUtility.HtmlAttributeEncode(spanName))
              .Append("\"></span>");

            sb.Append("<span class=\"eg-status-label\">")
              .Append(HttpUtility.HtmlEncode(displayLabel))
              .Append("</span>");

            sb.Append("</button>");
            return new MvcHtmlString(sb.ToString());
        }

        public static IHtmlString GridStatusToggleGroupOpen(this HtmlHelper html, string cssClass = null)
        {
            var cls = string.IsNullOrWhiteSpace(cssClass)
                ? "eg-status-group"
                : "eg-status-group " + cssClass;
            return new MvcHtmlString("<div class=\"" + HttpUtility.HtmlAttributeEncode(cls) + "\">");
        }

        public static IHtmlString GridStatusToggleGroupClose(this HtmlHelper html)
        {
            return new MvcHtmlString("</div>");
        }

        public static IHtmlString GridMeta(this HtmlHelper html, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return MvcHtmlString.Empty;
            }

            var sb = new StringBuilder(128);
            sb.Append("<span class=\"eg-meta-item\">");
            if (!string.IsNullOrWhiteSpace(label))
            {
                sb.Append("<span class=\"eg-meta-label\">")
                  .Append(HttpUtility.HtmlEncode(label))
                  .Append("</span> ");
            }
            sb.Append("<span class=\"eg-meta-value\">")
              .Append(HttpUtility.HtmlEncode(value))
              .Append("</span></span>");
            return new MvcHtmlString(sb.ToString());
        }
    }
}
