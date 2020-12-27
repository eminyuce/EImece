using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.AttributeHelper
{
    public class UnderConstAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (AppConfig.IsSiteUnderConstruction)
            {
                var ipAddress = HttpContext.Current.Request.UserHostAddress;
                var offlineHelper = new OfflineHelper(ipAddress, filterContext.HttpContext.Server.MapPath);
                if (offlineHelper.ThisUserShouldBeOffline)
                {
                    filterContext.HttpContext.Response.Redirect("/underconstruction");
                    return;
                }
            }
            //otherwise we let this through as normal
            base.OnActionExecuting(filterContext);
        }
    }
}