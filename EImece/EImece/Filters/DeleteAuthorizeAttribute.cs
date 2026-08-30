using EImece.Domain.Helpers;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace EImece.Filters
{
    public class DeleteAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAuthenticated)
            {
                base.OnAuthorization(filterContext);
                return;
            }

            var currentUser = filterContext.HttpContext.User;
            if (currentUser == null)
            {
                base.OnAuthorization(filterContext);
                return;
            }

            var roles = UserRoleHelper.GetDeletedRoles();
            bool isAllowed = roles.Any(role => currentUser.IsInRole(role));

            if (!isAllowed)
            {
                filterContext.Result = new RedirectToRouteResult(new
                    RouteValueDictionary(new { controller = "Error", action = "BadRequest" }));
            }
        }
    }
}
