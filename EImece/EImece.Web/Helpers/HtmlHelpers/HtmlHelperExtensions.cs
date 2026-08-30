using Resources;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace EImece.Web.Helpers.HtmlHelpers
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Bootstrap 4 pagination matching the site theme (.pagination / .page-item / .page-link).
        /// Returns empty markup when there is only a single page (or no items).
        /// </summary>
        public static MvcHtmlString BootstrapPager(this HtmlHelper helper, int currentPageIndex, Func<int, string> action, int totalItems, int pageSize = 10, int numberOfLinks = 5)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (totalItems <= 0 || pageSize <= 0)
            {
                return MvcHtmlString.Empty;
            }

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages <= 1)
            {
                return MvcHtmlString.Empty;
            }

            if (currentPageIndex < 1)
            {
                currentPageIndex = 1;
            }
            else if (currentPageIndex > totalPages)
            {
                currentPageIndex = totalPages;
            }

            if (numberOfLinks < 1)
            {
                numberOfLinks = 5;
            }

            var lastPageNumber = (int)Math.Ceiling(currentPageIndex / (double)numberOfLinks) * numberOfLinks;
            var firstPageNumber = lastPageNumber - (numberOfLinks - 1);
            if (lastPageNumber > totalPages)
            {
                lastPageNumber = totalPages;
            }

            var hasPreviousPage = currentPageIndex > 1;
            var hasNextPage = currentPageIndex < totalPages;

            var html = new StringBuilder();
            html.Append("<nav class=\"site-pagination\" aria-label=\"Pagination Navigation\">");
            html.Append("<ul class=\"pagination justify-content-center flex-wrap mb-0\">");

            html.Append(BuildPageItem(1, action, !hasPreviousPage, isActive: false, "«", "First"));
            html.Append(BuildPageItem(currentPageIndex - 1, action, !hasPreviousPage, isActive: false, "‹", "Previous"));

            for (int i = firstPageNumber; i <= lastPageNumber; i++)
            {
                var label = i.ToString(CultureInfo.InvariantCulture);
                html.Append(BuildPageItem(i, action, isDisabled: false, isActive: i == currentPageIndex, label, label));
            }

            html.Append(BuildPageItem(currentPageIndex + 1, action, !hasNextPage, isActive: false, "›", "Next"));
            html.Append(BuildPageItem(totalPages, action, !hasNextPage, isActive: false, "»", "Last"));

            html.Append("</ul></nav>");
            return MvcHtmlString.Create(html.ToString());
        }

        /// <summary>
        /// Validation summary that stays green for the admin success message
        /// ("İşleminiz başarıyla gerçekleşmiştir.") even though it is stored via ModelState.AddModelError.
        /// Real model-level errors still render as danger.
        /// </summary>
        public static MvcHtmlString AdminValidationSummary(this HtmlHelper html, bool excludePropertyErrors = true)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }

            return html.ValidationSummary(excludePropertyErrors, "", new
            {
                @class = GetAdminValidationSummaryCss(html.ViewData.ModelState),
                data_admin_success_message = AdminResource.SuccessfullySavedCompleted
            });
        }

        internal static string GetAdminValidationSummaryCss(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                return "alert alert-success";
            }

            var modelErrors = modelState
                .Where(kvp => string.IsNullOrEmpty(kvp.Key))
                .SelectMany(kvp => kvp.Value.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToList();

            if (modelErrors.Count == 0)
            {
                return "alert alert-success";
            }

            var successMessage = AdminResource.SuccessfullySavedCompleted;
            var isSuccessOnly = modelErrors.All(m =>
                string.Equals(m.Trim(), successMessage, StringComparison.Ordinal));

            return isSuccessOnly ? "alert alert-success" : "alert alert-danger";
        }

        private static string BuildPageItem(int index, Func<int, string> action, bool isDisabled, bool isActive, string linkText, string ariaLabel)
        {
            var li = new TagBuilder("li");
            li.AddCssClass("page-item");
            if (isActive)
            {
                li.AddCssClass("active");
                li.MergeAttribute("aria-current", "page");
            }
            if (isDisabled)
            {
                li.AddCssClass("disabled");
            }

            if (isActive)
            {
                var span = new TagBuilder("span");
                span.AddCssClass("page-link");
                span.SetInnerText(linkText);
                var sr = new TagBuilder("span");
                sr.AddCssClass("sr-only");
                sr.SetInnerText("(current)");
                span.InnerHtml += " " + sr;
                li.InnerHtml = span.ToString();
            }
            else
            {
                var a = new TagBuilder("a");
                a.AddCssClass("page-link");
                a.MergeAttribute("href", isDisabled ? "#" : action(index));
                a.MergeAttribute("aria-label", ariaLabel);
                if (isDisabled)
                {
                    a.MergeAttribute("tabindex", "-1");
                    a.MergeAttribute("aria-disabled", "true");
                }
                a.SetInnerText(linkText);
                li.InnerHtml = a.ToString();
            }

            return li.ToString();
        }
    }
}
