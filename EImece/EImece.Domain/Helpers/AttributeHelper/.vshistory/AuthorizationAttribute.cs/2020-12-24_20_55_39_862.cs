using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.AttributeHelper
{
    public class AuthorizationAttribute : AuthorizeAttribute
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
            Logger.Info("Roles+" + string.Join(",", roles));
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
