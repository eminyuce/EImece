using EImece.Domain.Factories.IFactories;
using System;
using System.Web;

namespace EImece.Domain.Factories
{
    public class HttpContextFactory : IHttpContextFactory
    {
        public HttpContextBase Create()
        {
            // After ConfigureAwait(false), HttpContext.Current can be null on continuations.
            var current = HttpContext.Current;
            return current == null ? null : new HttpContextWrapper(current);
        }

        public string GetCurrentUserId()
        {
            HttpContextBase c = Create();
            if (c?.User?.Identity != null && c.User.Identity.IsAuthenticated)
            {
                return c.User.Identity.Name;
            }
            else
            {
                return String.Empty;
            }
        }
    }
}