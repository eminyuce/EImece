using EImece.Domain.Factories.IFactories;
using System;
using System.Web;

namespace EImece.Domain.Factories
{
    public class HttpContextFactory : IHttpContextFactory
    {
        public HttpContextBase Create()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }

        public string GetCurrentUserId()
        {
            HttpContextBase c = Create();
            if (c.User.Identity.IsAuthenticated)
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