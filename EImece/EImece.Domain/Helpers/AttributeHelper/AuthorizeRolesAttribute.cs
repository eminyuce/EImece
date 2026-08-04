using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.AttributeHelper
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params string[] roles) : base()
        {
            Roles = string.Join(",", roles);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // TEMPORARY: allow unauthenticated admin browsing while BypassAdminAuth is enabled.
            if (EImece.Domain.AppConfig.BypassAdminAuth)
            {
                return true;
            }

            return base.AuthorizeCore(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // When AdminLogin is disabled, do not send users to the login page — redirect home.
            if (!EImece.Domain.AppConfig.AdminLoginEnabled && !EImece.Domain.AppConfig.BypassAdminAuth)
            {
                filterContext.Result = new RedirectResult("~/");
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
