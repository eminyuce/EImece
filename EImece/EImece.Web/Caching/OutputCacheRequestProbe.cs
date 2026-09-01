using EImece.Domain.Caching;
using System;
using System.Diagnostics;
using System.Web;

namespace EImece.Web.Caching
{
    /// <summary>
    /// Lightweight page-cache probe. Marks CustomOutputCache generations as misses.
    /// When IIS serves a cached HTML response without entering MVC, EndRequest records a hit.
    /// Never throws into the request pipeline.
    /// </summary>
    public static class OutputCacheRequestProbe
    {
        private const string StartKey = "eimece.oc.start";
        private const string BypassKey = "eimece.oc.bypass";
        private const string GenerateKey = "eimece.oc.generate";
        private const string MvcKey = "eimece.oc.mvc";

        public static void OnBeginRequest(HttpContext context)
        {
            if (context == null || context.Request == null)
            {
                return;
            }

            if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            context.Items[StartKey] = Stopwatch.StartNew();
        }

        public static void MarkMvcHandler(HttpContext context)
        {
            if (context == null || context.Handler == null)
            {
                return;
            }

            if (context.Handler is System.Web.Mvc.MvcHandler)
            {
                context.Items[MvcKey] = true;
            }
        }

        public static void MarkBypassed(HttpContextBase context)
        {
            if (context != null)
            {
                context.Items[BypassKey] = true;
            }
        }

        public static void MarkPageGeneration(HttpContextBase context)
        {
            if (context != null)
            {
                context.Items[GenerateKey] = true;
            }
        }

        public static void OnEndRequest(HttpContext context)
        {
            if (context == null)
            {
                return;
            }

            var sw = context.Items[StartKey] as Stopwatch;
            if (sw == null)
            {
                return;
            }

            if (!sw.IsRunning)
            {
                return;
            }

            sw.Stop();
            var ticks = sw.ElapsedTicks;

            if (context.Items[BypassKey] != null)
            {
                return;
            }

            if (context.Items[GenerateKey] != null)
            {
                CacheDiagnostics.RecordOutputMiss(ticks);
                return;
            }

            if (!LooksLikeCachedHtmlHit(context))
            {
                return;
            }

            CacheDiagnostics.RecordOutputHit(ticks);
        }

        private static bool LooksLikeCachedHtmlHit(HttpContext context)
        {
            if (context.Response == null || context.Request == null)
            {
                return false;
            }

            if (context.Response.StatusCode != 200)
            {
                return false;
            }

            var path = context.Request.Path ?? "";
            if (path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/account", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/customers", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/content", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/scripts", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/bundles", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/media", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/images", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var contentType = context.Response.ContentType ?? "";
            if (contentType.Length == 0)
            {
                return true;
            }

            return contentType.IndexOf("html", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
