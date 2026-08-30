using System.Web;
using System.Web.Mvc;

namespace EImece.Filters
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params string[] roles) : base()
        {
            Roles = string.Join(",", roles);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // When AdminLogin is disabled, do not send users to the login page — redirect home.
            if (!EImece.Domain.AppConfig.AdminLoginEnabled)
            {
                filterContext.Result = new RedirectResult("~/");
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
