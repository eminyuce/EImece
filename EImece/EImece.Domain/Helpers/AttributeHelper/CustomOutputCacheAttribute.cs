using System.Web.Mvc;

namespace EImece.Domain.Helpers.AttributeHelper
{
    public class CustomOutputCacheAttribute : OutputCacheAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;

            if (httpContext.User.Identity.IsAuthenticated)
            {
                httpContext.Response.Cache.SetNoServerCaching();
                httpContext.Response.Cache.SetNoStore();
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;

            if (httpContext.User.Identity.IsAuthenticated)
            {
                httpContext.Response.Cache.SetNoServerCaching();
                httpContext.Response.Cache.SetNoStore();
            }
            else
            {
                base.OnResultExecuting(filterContext);
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            // Never cache error responses (avoids blank 500 pages being served from output cache).
            if (filterContext != null
                && filterContext.HttpContext != null
                && filterContext.HttpContext.Response.StatusCode >= 400)
            {
                filterContext.HttpContext.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                filterContext.HttpContext.Response.Cache.SetNoStore();
                filterContext.HttpContext.Response.Cache.SetNoServerCaching();
            }

            base.OnResultExecuted(filterContext);
        }
    }
}