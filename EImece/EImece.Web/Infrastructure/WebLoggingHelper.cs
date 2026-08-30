using EImece.Domain.Observability.Logging;
using NLog;
using Serilog.Context;
using System.Diagnostics;
using System.Web;

namespace EImece.Web.Infrastructure
{
    public static class WebLoggingHelper
    {
        private const string CorrelationIdProperty = "CorrelationId";
        private const string TraceIdProperty = "TraceId";
        private const string SpanIdProperty = "SpanId";

        public static void EnrichFromHttpContext()
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return;
            }

            var correlationId = CorrelationIdContext.Ensure();
            var activity = Activity.Current;
            var traceId = activity?.TraceId.ToString();
            var spanId = activity?.SpanId.ToString();

            LogContext.PushProperty(CorrelationIdProperty, correlationId);
            LogContext.PushProperty("RequestId", context.Items["RequestId"]);
            LogContext.PushProperty("ClientIp", context.Request.UserHostAddress);
            LogContext.PushProperty("RequestPath", context.Request.Url?.AbsolutePath);
            LogContext.PushProperty("HttpMethod", context.Request.HttpMethod);

            if (!string.IsNullOrEmpty(traceId))
            {
                LogContext.PushProperty(TraceIdProperty, traceId);
            }

            if (!string.IsNullOrEmpty(spanId))
            {
                LogContext.PushProperty(SpanIdProperty, spanId);
            }

            // NLog scope properties for layouts that read ${scopeproperty:item=...}
            ScopeContext.PushProperty(CorrelationIdProperty, correlationId);
            ScopeContext.PushProperty("RequestPath", context.Request.Url?.AbsolutePath);
            ScopeContext.PushProperty("HttpMethod", context.Request.HttpMethod);
            ScopeContext.PushProperty("ClientIp", context.Request.UserHostAddress);

            if (!string.IsNullOrEmpty(traceId))
            {
                ScopeContext.PushProperty(TraceIdProperty, traceId);
            }

            if (!string.IsNullOrEmpty(spanId))
            {
                ScopeContext.PushProperty(SpanIdProperty, spanId);
            }

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                LogContext.PushProperty("UserId", context.User.Identity.Name);
                ScopeContext.PushProperty("UserId", context.User.Identity.Name);
            }
        }
    }
}
