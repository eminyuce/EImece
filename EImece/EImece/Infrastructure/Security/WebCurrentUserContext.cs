using EImece.Domain.Abstractions;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Threading;
using System.Web;

namespace EImece.Infrastructure.Security
{
    public class WebCurrentUserContext : ICurrentUserContext
    {
        public string GetCurrentUserId()
        {
            var httpContext = HttpContext.Current;
            if (httpContext?.User?.Identity != null && httpContext.User.Identity.IsAuthenticated)
            {
                return httpContext.User.Identity.GetUserId();
            }

            var principal = Thread.CurrentPrincipal as ClaimsPrincipal;
            if (principal?.Identity != null && principal.Identity.IsAuthenticated)
            {
                return principal.Identity.GetUserId();
            }

            return null;
        }

        public bool IsAuthenticated
        {
            get
            {
                var httpContext = HttpContext.Current;
                if (httpContext?.User?.Identity != null)
                {
                    return httpContext.User.Identity.IsAuthenticated;
                }

                var principal = Thread.CurrentPrincipal;
                return principal?.Identity != null && principal.Identity.IsAuthenticated;
            }
        }

        public bool IsInRole(string role)
        {
            var httpContext = HttpContext.Current;
            if (httpContext?.User != null)
            {
                return httpContext.User.IsInRole(role);
            }

            var principal = Thread.CurrentPrincipal;
            return principal?.IsInRole(role) ?? false;
        }
    }
}
