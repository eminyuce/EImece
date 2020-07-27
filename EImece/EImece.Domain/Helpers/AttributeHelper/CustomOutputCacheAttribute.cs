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
    }
}