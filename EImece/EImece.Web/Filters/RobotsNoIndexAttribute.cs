using EImece.Domain.Helpers;
using System;
using System.Web.Mvc;

namespace EImece.Web.Filters
{
    /// <summary>
    /// When search engine indexing is disabled, emits X-Robots-Tag on every MVC response.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RobotsNoIndexAttribute : ActionFilterAttribute
    {
        private const string XRobotsTagHeader = "X-Robots-Tag";
        private const string NoIndexDirectives = "noindex, nofollow, noarchive";

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext?.HttpContext?.Response == null)
            {
                return;
            }

            if (filterContext.IsChildAction)
            {
                return;
            }

            if (!SeoSettings.AllowIndexing)
            {
                var response = filterContext.HttpContext.Response;
                if (response.Headers != null && response.Headers[XRobotsTagHeader] != null)
                {
                    return;
                }
                response.AppendHeader(XRobotsTagHeader, NoIndexDirectives);
            }

            base.OnResultExecuting(filterContext);
        }
    }
}
