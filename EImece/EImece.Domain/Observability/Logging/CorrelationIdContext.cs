using System;
using System.Diagnostics;
using System.Threading;

namespace EImece.Domain.Observability.Logging
{
    public static class CorrelationIdContext
    {
        public const string HeaderName = "X-Correlation-Id";
        public const string TraceParentHeaderName = "traceparent";
        public const string TraceStateHeaderName = "tracestate";

        private static readonly AsyncLocal<string> _current = new AsyncLocal<string>();
        private static readonly AsyncLocal<string> _traceParent = new AsyncLocal<string>();
        private static readonly AsyncLocal<string> _traceState = new AsyncLocal<string>();

        public static string Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }

        public static string TraceParent
        {
            get => _traceParent.Value;
            set => _traceParent.Value = value;
        }

        public static string TraceState
        {
            get => _traceState.Value;
            set => _traceState.Value = value;
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
