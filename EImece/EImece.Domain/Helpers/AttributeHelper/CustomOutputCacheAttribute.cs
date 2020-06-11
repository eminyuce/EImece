using System.Web.Mvc;

namespace EImece.Domain.Helpers.AttributeHelper
{
    public class CustomOutputCacheAttribute : OutputCacheAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.HttpContext.Response.Cache.SetNoServerCaching();
                filterContext.HttpContext.Response.Cache.SetNoStore();
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.HttpContext.Response.Cache.SetNoServerCaching();
                filterContext.HttpContext.Response.Cache.SetNoStore();
            }
            else
            {
                base.OnResultExecuting(filterContext);
            }
        }

    }
}
