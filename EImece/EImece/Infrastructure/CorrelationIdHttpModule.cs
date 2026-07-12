using EImece.Domain.Observability.Logging;
using System;
using System.Web;

namespace EImece.Infrastructure
{
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
            var correlationId = request.Headers[CorrelationIdContext.HeaderName];

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            CorrelationIdContext.Current = correlationId;
            application.Context.Items["RequestId"] = Guid.NewGuid().ToString("N");
            application.Response.Headers[CorrelationIdContext.HeaderName] = correlationId;
        }

        private static void OnEndRequest(object sender, EventArgs e)
        {
        }
    }
}
