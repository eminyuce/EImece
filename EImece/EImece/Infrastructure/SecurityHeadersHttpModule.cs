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
            response.Headers["X-Content-Type-Options"] = "nosniff";
            response.Headers["X-Frame-Options"] = "SAMEORIGIN";
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            response.Headers["X-XSS-Protection"] = "1; mode=block";
        }
    }
}
