using System;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;

namespace EImece.Web.Filters
{
    /// <summary>
    /// Validates antiforgery tokens for JSON/AJAX posts that send the token in a header
    /// (RequestVerificationToken / X-RequestVerificationToken) or as a form field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateJsonAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            var request = filterContext.HttpContext.Request;
            if (!IsStateChangingMethod(request.HttpMethod))
            {
                return;
            }

            var cookieToken = request.Cookies[AntiForgeryConfig.CookieName]?.Value;
            var requestToken = request.Headers["RequestVerificationToken"]
                ?? request.Headers["X-RequestVerificationToken"]
                ?? request.Form["__RequestVerificationToken"];

            try
            {
                AntiForgery.Validate(cookieToken, requestToken);
            }
            catch (HttpAntiForgeryException)
            {
                // Prefer 403 over an unhandled 500 for missing/invalid AJAX antiforgery tokens.
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Invalid antiforgery token.");
            }
        }

        private static bool IsStateChangingMethod(string httpMethod)
        {
            return string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase)
                || string.Equals(httpMethod, "PUT", StringComparison.OrdinalIgnoreCase)
                || string.Equals(httpMethod, "DELETE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(httpMethod, "PATCH", StringComparison.OrdinalIgnoreCase);
        }
    }
}
