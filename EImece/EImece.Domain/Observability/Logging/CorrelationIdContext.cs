using System;
using System.Web;

namespace EImece.Domain.Observability.Logging
{
    public static class CorrelationIdContext
    {
        public const string HeaderName = "X-Correlation-Id";
        public const string HttpContextItemKey = "CorrelationId";

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

        public static string Ensure()
        {
            var correlationId = Current;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
                Current = correlationId;
            }

            return correlationId;
        }
    }
}
