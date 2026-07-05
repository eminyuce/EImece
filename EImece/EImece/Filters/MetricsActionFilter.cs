using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Metrics;
using System.Diagnostics;
using System.Web.Mvc;

namespace EImece.Filters
{
    public sealed class MetricsActionFilter : IActionFilter
    {
        private readonly IApplicationMetrics _metrics;
        private readonly ObservabilityOptions _options;
        private const string StopwatchKey = "MetricsActionStopwatch";

        public MetricsActionFilter(IApplicationMetrics metrics)
        {
            _metrics = metrics;
            _options = ObservabilityOptions.FromAppConfig();
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!_options.EnableMetrics)
            {
                return;
            }

            filterContext.HttpContext.Items[StopwatchKey] = Stopwatch.StartNew();
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!_options.EnableMetrics)
            {
                return;
            }

            var stopwatch = filterContext.HttpContext.Items[StopwatchKey] as Stopwatch;
            if (stopwatch == null)
            {
                return;
            }

            stopwatch.Stop();
            var route = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "." + filterContext.ActionDescriptor.ActionName;
            var success = filterContext.Exception == null && filterContext.HttpContext.Response.StatusCode < 500;
            _metrics.RecordRequest(route, stopwatch.ElapsedMilliseconds, success);
        }
    }
}
