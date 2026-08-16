using EImece.Domain.Observability.Logging;
using System;
using System.Web;

namespace EImece.Infrastructure
{
    /// <summary>
    /// Accepts or generates X-Correlation-Id, prefers W3C traceparent when present,
    /// and stores values in HttpContext.Items for ActionFilters and log enrichers.
    /// </summary>
    public sealed class CorrelationIdHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
            context.EndRequest += OnEndRequest;
        }

        public void Dispose()
        {
        }

        private static void OnBeginRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            var request = application.Context.Request;

            var traceParent = request.Headers[CorrelationIdContext.TraceParentHeaderName];
            if (!string.IsNullOrWhiteSpace(traceParent))
            {
                CorrelationIdContext.TraceParent = traceParent.Trim();
                var traceState = request.Headers[CorrelationIdContext.TraceStateHeaderName];
                if (!string.IsNullOrWhiteSpace(traceState))
                {
                    CorrelationIdContext.TraceState = traceState.Trim();
                }
            }

            var correlationId = request.Headers[CorrelationIdContext.HeaderName];
            if (string.IsNullOrWhiteSpace(correlationId) && !string.IsNullOrWhiteSpace(traceParent))
            {
                // W3C: version-traceid-spanid-flags — reuse trace-id as correlation when header absent.
                var parts = traceParent.Trim().Split('-');
                if (parts.Length >= 2 && parts[1].Length == 32)
                {
                    correlationId = parts[1];
                }
            }

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            CorrelationIdContext.Current = correlationId;
            application.Context.Items["RequestId"] = Guid.NewGuid().ToString("N");

            // Bind CorrelationId to NLog ScopeContext (NLog 5+ standard)
            NLog.ScopeContext.PushProperty(CorrelationIdContext.HttpContextItemKey, correlationId);

            try
            {
                application.Response.Headers[CorrelationIdContext.HeaderName] = correlationId;
            }
            catch (HttpException)
            {
                // Headers may be restricted for some pipeline stages; Items still hold the value.
            }
        }

        private static void OnEndRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            var correlationId = CorrelationIdContext.Current;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(application.Response.Headers[CorrelationIdContext.HeaderName]))
                {
                    application.Response.Headers[CorrelationIdContext.HeaderName] = correlationId;
                }
            }
            catch (Exception)
            {
                // Headers may be restricted for some pipeline stages; Items still hold the value.
            }
        }
    }
}
