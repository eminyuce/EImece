using System;
using System.Diagnostics;
using System.Web;

namespace EImece.Domain.Observability.Logging
{
    public static class CorrelationIdContext
    {
        public const string HeaderName = "X-Correlation-Id";
        public const string TraceParentHeaderName = "traceparent";
        public const string TraceStateHeaderName = "tracestate";
        public const string HttpContextItemKey = "CorrelationId";
        public const string TraceParentItemKey = "TraceParent";
        public const string TraceStateItemKey = "TraceState";

        public static string Current
        {
            get
            {
                var context = HttpContext.Current;
                if (context == null)
                {
                    return null;
                }

                return context.Items[HttpContextItemKey] as string;
            }
            set
            {
                var context = HttpContext.Current;
                if (context != null)
                {
                    context.Items[HttpContextItemKey] = value;
                }
            }
        }

        public static string TraceParent
        {
            get
            {
                var context = HttpContext.Current;
                return context?.Items[TraceParentItemKey] as string;
            }
            set
            {
                var context = HttpContext.Current;
                if (context != null)
                {
                    context.Items[TraceParentItemKey] = value;
                }
            }
        }

        public static string TraceState
        {
            get
            {
                var context = HttpContext.Current;
                return context?.Items[TraceStateItemKey] as string;
            }
            set
            {
                var context = HttpContext.Current;
                if (context != null)
                {
                    context.Items[TraceStateItemKey] = value;
                }
            }
        }

        public static string Ensure()
        {
            var correlationId = Current;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                // Prefer W3C trace-id when an Activity is already present.
                var activity = Activity.Current;
                if (activity != null && activity.TraceId != default)
                {
                    correlationId = activity.TraceId.ToString();
                }
                else
                {
                    correlationId = Guid.NewGuid().ToString("N");
                }

                Current = correlationId;
            }

            return correlationId;
        }

        public static bool TryGetParentContext(out ActivityContext parentContext)
        {
            parentContext = default(ActivityContext);
            var traceParent = TraceParent;
            if (string.IsNullOrWhiteSpace(traceParent))
            {
                return false;
            }

            return ActivityContext.TryParse(traceParent, TraceState, out parentContext);
        }
    }
}
