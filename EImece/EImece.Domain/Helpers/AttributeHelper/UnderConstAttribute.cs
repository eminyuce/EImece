using EImece.Domain.Services.IServices;
using System;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.AttributeHelper
{
    public class UnderConstAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var controller = filterContext.ActionDescriptor?.ControllerDescriptor?.ControllerName ?? string.Empty;
            var action = filterContext.ActionDescriptor?.ActionName ?? string.Empty;
            var area = filterContext.RouteData?.DataTokens["area"]?.ToString() ?? string.Empty;

            // Allow ErrorController, UnderConstructionController, Admin area, and Admin login/2FA/logoff
            if (controller.Equals("UnderConstruction", StringComparison.OrdinalIgnoreCase) ||
                controller.Equals("Error", StringComparison.OrdinalIgnoreCase) ||
                area.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                (controller.Equals("Account", StringComparison.OrdinalIgnoreCase) &&
                 (action.Equals("AdminLogin", StringComparison.OrdinalIgnoreCase) ||
                  action.Equals("VerifyAuthenticator", StringComparison.OrdinalIgnoreCase) ||
                  action.Equals("LogOff", StringComparison.OrdinalIgnoreCase))))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            ISettingService settingService = null;
            try
            {
                settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
            }
            catch
            {
                settingService = null;
            }

            var isUnderConstruction = settingService != null
                ? settingService.GetSettingByKey(Constants.IsSiteUnderConstruction).ToBool(false)
                : false;
            var user = filterContext.HttpContext.User;
            var isAuth = user != null && user.Identity != null && user.Identity.IsAuthenticated;
            var isAdmin = isAuth && (user.IsInRole(Constants.AdministratorRole) || user.IsInRole(Constants.EditorRole));

            if (isUnderConstruction)
            {
                // If an offline file exists, check IP whitelist
                var ipAddress = filterContext.HttpContext.Request?.UserHostAddress;
                var offlineHelper = new OfflineHelper(ipAddress, filterContext.HttpContext.Server.MapPath);
                if (OfflineHelper.OfflineData != null && !offlineHelper.ThisUserShouldBeOffline)
                {
                    // IP is explicitly whitelisted in offline file
                    base.OnActionExecuting(filterContext);
                    return;
                }

                // If user is already authenticated as an Admin/Editor, allow access
                if (isAdmin)
                {
                    base.OnActionExecuting(filterContext);
                    return;
                }

                if (filterContext.IsChildAction)
                {
                    filterContext.Result = new EmptyResult();
                    return;
                }

                filterContext.Result = new RedirectResult("/underconstruction");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}