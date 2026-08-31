using EImece.Domain.Observability.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;

namespace EImece.Web.Infrastructure
{
    public static class WebLoggingHelper
    {
        private const string CorrelationIdProperty = "CorrelationId";
        private const string TraceIdProperty = "TraceId";
        private const string SpanIdProperty = "SpanId";

        public static Dictionary<string, object> BuildScopeStateFromHttpContext()
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return null;
            }

            var correlationId = CorrelationIdContext.Ensure();
            var activity = Activity.Current;
            var state = new Dictionary<string, object>
            {
                [CorrelationIdProperty] = correlationId,
                ["RequestId"] = context.Items["RequestId"],
                ["ClientIp"] = context.Request.UserHostAddress,
                ["RequestPath"] = context.Request.Url?.AbsolutePath,
                ["HttpMethod"] = context.Request.HttpMethod,
            };

            if (activity != null)
            {
                state[TraceIdProperty] = activity.TraceId.ToString();
                state[SpanIdProperty] = activity.SpanId.ToString();
            }

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                state["UserId"] = context.User.Identity.Name;
            }

            return state;
        }

        public static void EnrichFromHttpContext()
        {
            var state = BuildScopeStateFromHttpContext();
            if (state == null)
            {
                return;
            }

            foreach (var pair in state)
            {
                if (pair.Value != null)
                {
                    NLog.ScopeContext.PushProperty(pair.Key, pair.Value);
                }
            }
        }

        public static IDisposable BeginRequestScope(ILogger logger)
        {
            var state = BuildScopeStateFromHttpContext();
            if (state == null)
            {
                return NullScope.Instance;
            }

            foreach (var pair in state)
            {
                if (pair.Value != null)
                {
                    NLog.ScopeContext.PushProperty(pair.Key, pair.Value);
                }
            }

            return logger?.BeginScope(state) ?? NullScope.Instance;
        }

        private sealed class NullScope : System.IDisposable
        {
            internal static readonly NullScope Instance = new NullScope();
            public void Dispose() { }
        }
    }
}
