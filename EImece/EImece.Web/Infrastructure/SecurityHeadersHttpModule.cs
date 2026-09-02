using System;
using System.Web;

namespace EImece.Web.Infrastructure
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
            var context = HttpContext.Current;
            var response = context?.Response;
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

            // Content-Security-Policy (CSP)
            if (response.Headers["Content-Security-Policy"] == null)
            {
                const string csp = "default-src 'self'; " +
                                   "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://www.google.com/recaptcha/ https://www.gstatic.com/recaptcha/ https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://static.iyzipay.com https://www.googletagmanager.com https://www.google-analytics.com https://static.getbutton.io; " +
                                   "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://static.getbutton.io; " +
                                   "font-src 'self' data: https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
                                   "img-src 'self' data: blob: https: http:; " +
                                   "connect-src 'self' https://sandbox-api.iyzipay.com https://api.iyzipay.com https://www.google.com https://www.google-analytics.com https://www.googletagmanager.com https://region1.google-analytics.com https://static.getbutton.io; " +
                                   "frame-src 'self' https://www.google.com/recaptcha/ https://sandbox-api.iyzipay.com https://api.iyzipay.com https://www.googletagmanager.com; " +
                                   "frame-ancestors 'self'; " +
                                   "base-uri 'self'; " +
                                   "form-action 'self' https://sandbox-api.iyzipay.com https://api.iyzipay.com;";
                response.Headers["Content-Security-Policy"] = csp;
            }

            // Optimize bundled, content-hashed assets with immutable Cache-Control
            var path = context.Request?.Path;
            if (!string.IsNullOrEmpty(path) && path.StartsWith("/bundles/", StringComparison.OrdinalIgnoreCase) && response.StatusCode == 200)
            {
                response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
            }
        }
    }
}
