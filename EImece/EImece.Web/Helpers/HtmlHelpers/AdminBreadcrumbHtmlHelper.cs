using System;
using System.Text;
using System.Web.Mvc;

namespace EImece.Web.Helpers.HtmlHelpers
{
    /// <summary>
    /// Renders a consistent Bootstrap 5 breadcrumb trail for admin pages:
    /// Home &gt; [ancestor] &gt; current page. Every admin list page should start
    /// with this helper so navigation formatting stays uniform.
    /// </summary>
    public static class AdminBreadcrumbHtmlHelper
    {
        public static MvcHtmlString AdminBreadcrumb(this HtmlHelper htmlHelper, string currentTitle, params (string Label, string Url)[] ancestors)
        {
            if (string.IsNullOrWhiteSpace(currentTitle))
            {
                return MvcHtmlString.Empty;
            }

            var url = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            string homeUrl = url.Action("Index", "Dashboard", new { area = "admin" });
            string homeLabel = Resources.AdminResource.AdminHome;

            var sb = new StringBuilder();
            sb.Append("<nav aria-label=\"breadcrumb\" class=\"admin-breadcrumb\">");
            sb.Append("<ol class=\"breadcrumb\">");
            sb.AppendFormat(
                "<li class=\"breadcrumb-item\"><a href=\"{0}\"><i class=\"fa-solid fa-house\"></i><span> {1}</span></a></li>",
                Encode(homeUrl),
                Encode(homeLabel));

            if (ancestors != null)
            {
                foreach (var ancestor in ancestors)
                {
                    if (string.IsNullOrWhiteSpace(ancestor.Label))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(ancestor.Url))
                    {
                        sb.AppendFormat("<li class=\"breadcrumb-item\"><span>{0}</span></li>", Encode(ancestor.Label));
                    }
                    else
                    {
                        sb.AppendFormat(
                            "<li class=\"breadcrumb-item\"><a href=\"{0}\"><span>{1}</span></a></li>",
                            Encode(ancestor.Url),
                            Encode(ancestor.Label));
                    }
                }
            }

            sb.AppendFormat(
                "<li class=\"breadcrumb-item active\" aria-current=\"page\">{0}</li>",
                Encode(currentTitle));
            sb.Append("</ol>");
            sb.Append("</nav>");

            return new MvcHtmlString(sb.ToString());
        }

        private static string Encode(string value)
        {
            return (value ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
