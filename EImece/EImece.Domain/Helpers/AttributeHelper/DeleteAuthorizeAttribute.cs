using System.Web.Mvc;
using System.Web.Routing;

namespace EImece.Domain.Helpers.AttributeHelper
{
    public class DeleteAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                var currentUser = filterContext.HttpContext.User;
                var roles = UserRoleHelper.GetDeletedRoles();
                foreach (var role in roles)
                {
                    if (!currentUser.IsInRole(role))
                    {
                        filterContext.Result = new RedirectToRouteResult(new
                        RouteValueDictionary(new { controller = "Error", action = "BadRequest" }));

                        // base.OnAuthorization(filterContext); //returns to login url
                    }
                }
            }
        }
    }
}