using System;
using System.Diagnostics;
using System.Web;

namespace EImece.Domain.Observability.Logging
{
    public sealed class RequestLogContext : IDisposable
    {
        private readonly Stopwatch _stopwatch;

        public RequestLogContext()
        {
            _stopwatch = Stopwatch.StartNew();
            CorrelationId = CorrelationIdContext.Ensure();
            RequestId = Guid.NewGuid().ToString("N");
        }

        public string CorrelationId { get; }

        public string RequestId { get; }

        public long ElapsedMilliseconds
        {
            get { return _stopwatch.ElapsedMilliseconds; }
        }

        public string UserId
        {
            get
            {
                var context = HttpContext.Current;
                if (context?.User?.Identity?.IsAuthenticated == true)
                {
                    return context.User.Identity.Name;
                }

                return null;
            }
        }

        public string ClientIp
        {
            get
            {
                var context = HttpContext.Current;
                return context?.Request?.UserHostAddress;
            }
        }

        public string RequestPath
        {
            get
            {
                var context = HttpContext.Current;
                return context?.Request?.Url?.AbsolutePath;
            }
        }

        public string HttpMethod
        {
            get
            {
                var context = HttpContext.Current;
                return context?.Request?.HttpMethod;
            }
        }

        public void Dispose()
        {
            _stopwatch.Stop();
        }
    }
}
