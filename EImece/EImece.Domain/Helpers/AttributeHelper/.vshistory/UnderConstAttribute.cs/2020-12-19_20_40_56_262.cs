using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace EImece.Domain.Helpers.AttributeHelper
{
    public class UnderConstAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ipAddress = HttpContext.Current.Request.UserHostAddress;
            var offlineHelper = new OfflineHelper(ipAddress,filterContext.HttpContext.Server.MapPath);
            if (AppConfig.IsSiteUnderConstruction)
            {
                filterContext.HttpContext.Response.Redirect("/underconstruction");
                return;
            }
            //otherwise we let this through as normal
            base.OnActionExecuting(filterContext);
        }
    }
}
