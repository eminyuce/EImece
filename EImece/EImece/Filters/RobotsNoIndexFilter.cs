using EImece.Domain.Helpers;
using System.Web.Mvc;

namespace EImece.Filters
{
    /// <summary>
    /// When search engine indexing is disabled, emits X-Robots-Tag on every MVC response.
    /// </summary>
    public sealed class RobotsNoIndexFilter : ActionFilterAttribute
    {
        private const string XRobotsTagHeader = "X-Robots-Tag";
        private const string NoIndexDirectives = "noindex, nofollow, noarchive";

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext?.HttpContext?.Response == null)
            {
                return;
            }

            if (!SeoSettings.AllowIndexing)
            {
                // AppendHeader works in classic and integrated pipeline modes.
                filterContext.HttpContext.Response.AppendHeader(XRobotsTagHeader, NoIndexDirectives);
            }

            base.OnResultExecuting(filterContext);
        }
    }
}
