using System;
using System.Web;

namespace EImece.Infrastructure
{
    public sealed class SecurityHeadersHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        public void Dispose()
        {
        }

        private static void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var response = HttpContext.Current?.Response;
            if (response == null)
            {
                return;
            }

            response.Headers.Remove("Server");
            response.Headers.Remove("X-Frame-Options");
            response.Headers.Remove("X-Content-Type-Options");
            response.AddHeader("X-Content-Type-Options", "nosniff");
            response.AddHeader("X-Frame-Options", "SAMEORIGIN");
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            response.Headers["X-XSS-Protection"] = "1; mode=block";
        }
    }
}
