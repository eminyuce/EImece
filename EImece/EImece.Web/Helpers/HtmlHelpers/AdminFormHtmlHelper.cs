using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace EImece.Web.Helpers.HtmlHelpers
{
    public static class AdminFormHtmlHelper
    {
        public const string RequiredMarkerCssClass = "admin-required-marker";

        /// <summary>
        /// Renders a form label and appends a red bold asterisk when the property has a [Required] attribute.
        /// </summary>
        public static MvcHtmlString AdminLabelFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            object htmlAttributes = null)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }

            var label = html.LabelFor(expression, htmlAttributes);
            if (!IsExplicitlyRequired(html, expression))
            {
                return label;
            }

            return MvcHtmlString.Create(AppendRequiredMarkerToLabel(label.ToHtmlString()));
        }

        public static MvcHtmlString AdminRequiredMarker(this HtmlHelper html)
        {
            return MvcHtmlString.Create(BuildRequiredMarkerHtml());
        }

        internal static bool IsExplicitlyRequired<TModel, TValue>(
            HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            if (metadata?.ContainerType == null || string.IsNullOrEmpty(metadata.PropertyName))
            {
                return false;
            }

            var property = metadata.ContainerType.GetProperty(
                metadata.PropertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            return property != null
                && property.GetCustomAttributes(typeof(RequiredAttribute), inherit: true).Any();
        }

        internal static string BuildRequiredMarkerHtml()
        {
            return "<span class=\"" + RequiredMarkerCssClass + "\" aria-hidden=\"true\">*</span>";
        }

        /// <summary>
        /// Inserts the required marker inside the label so it stays inline with label text
        /// and does not become a separate Bootstrap grid column sibling.
        /// </summary>
        internal static string AppendRequiredMarkerToLabel(string labelHtml)
        {
            if (string.IsNullOrEmpty(labelHtml))
            {
                return labelHtml;
            }

            if (labelHtml.IndexOf(RequiredMarkerCssClass, StringComparison.Ordinal) >= 0)
            {
                return labelHtml;
            }

            var closeTag = "</label>";
            var closeIndex = labelHtml.LastIndexOf(closeTag, StringComparison.OrdinalIgnoreCase);
            if (closeIndex < 0)
            {
                return labelHtml + " " + BuildRequiredMarkerHtml();
            }

            var beforeClose = labelHtml.Substring(0, closeIndex);
            if (EndsWithRequiredAsterisk(beforeClose))
            {
                return labelHtml;
            }

            return beforeClose + " " + BuildRequiredMarkerHtml() + closeTag;
        }

        private static bool EndsWithRequiredAsterisk(string labelInnerHtml)
        {
            if (string.IsNullOrWhiteSpace(labelInnerHtml))
            {
                return false;
            }

            var gtIndex = labelInnerHtml.LastIndexOf('>');
            var innerText = gtIndex >= 0
                ? labelInnerHtml.Substring(gtIndex + 1)
                : labelInnerHtml;

            return innerText.TrimEnd().EndsWith("*", StringComparison.Ordinal);
        }
    }
}
