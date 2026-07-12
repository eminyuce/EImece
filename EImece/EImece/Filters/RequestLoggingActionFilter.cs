using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Observability.Metrics;
using System;
using System.Web.Mvc;

namespace EImece.Filters
{
    public sealed class RequestLoggingActionFilter : IActionFilter
    {
        private readonly ObservabilityOptions _options;
        private const string StopwatchKey = "RequestLoggingStopwatch";

        public RequestLoggingActionFilter()
        {
            _options = ObservabilityOptions.FromAppConfig();
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!_options.EnableRequestLogging)
            {
                return;
            }

            filterContext.HttpContext.Items[StopwatchKey] = System.Diagnostics.Stopwatch.StartNew();
            StructuredLoggingBootstrap.EnrichFromHttpContext();
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!_options.EnableRequestLogging)
            {
                return;
            }

            var stopwatch = filterContext.HttpContext.Items[StopwatchKey] as System.Diagnostics.Stopwatch;
            if (stopwatch == null)
            {
                return;
            }

            stopwatch.Stop();
            StructuredLoggingBootstrap.LogRequestCompleted(stopwatch.ElapsedMilliseconds, filterContext.HttpContext.Response.StatusCode);

            if (filterContext.Exception != null)
            {
                StructuredLoggingBootstrap.LogException(filterContext.Exception, "Unhandled MVC action exception");
            }
        }
    }
}
