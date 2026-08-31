using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using EImece.Domain.Observability.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace EImece.Web.Filters
{
    public class AuthorizationAttribute : AuthorizeAttribute
    {
        private static ILogger Logger =>
            LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(AuthorizationAttribute))
            ?? NullLogger.Instance;

        protected override bool AuthorizeCore(HttpContextBase actionContext)
        {
            ClaimsPrincipal currentPrincipal = HttpContext.Current.User as ClaimsPrincipal;
            if (currentPrincipal != null && CheckRoles(currentPrincipal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckRoles(ClaimsPrincipal principal)
        {
            string[] roles = RolesSplit;
            Logger.LogInformation("Roles=" + string.Join(",", roles));
            if (roles.Length == 0) return true;
            return roles.Any(principal.IsInRole);
        }

        protected string[] RolesSplit
        {
            get { return SplitStrings(Roles); }
        }

        protected static string[] SplitStrings(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new string[0];
            var result = input.Split(',').Where(s => !String.IsNullOrWhiteSpace(s.Trim()));
            return result.Select(s => s.Trim()).ToArray();
        }
    }
}
