using EImece.Domain.Observability;
using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Observability.Metrics;
using EImece.Domain.Observability.Telemetry;
using System.Diagnostics;
using System.Web.Mvc;

namespace EImece.Filters
{
    /// <summary>
    /// Global MVC ActionFilter that starts a Server Activity, records application + OTel metrics,
    /// and propagates correlation into Activity tags. Controllers stay free of telemetry boilerplate.
    /// </summary>
    public class TelemetryActionFilter : IActionFilter
    {
        private readonly IApplicationMetrics _metrics;
        private readonly ObservabilityOptions _options;
        private const string StopwatchKey = "TelemetryActionStopwatch";
        private const string ActivityKey = "TelemetryActionActivity";

        public TelemetryActionFilter(IApplicationMetrics metrics, ObservabilityOptions options = null)
        {
            _metrics = metrics;
            _options = options ?? ObservabilityOptions.FromAppConfig();
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext?.HttpContext == null)
            {
                return;
            }

            StructuredLoggingBootstrap.EnrichFromHttpContext();

            if (!_options.EnableTracing && !_options.EnableMetrics)
            {
                return;
            }

            filterContext.HttpContext.Items[StopwatchKey] = Stopwatch.StartNew();

            if (!_options.EnableTracing)
            {
                return;
            }

            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;
            var route = controller + "." + action;
            var httpMethod = filterContext.HttpContext.Request.HttpMethod;

            Activity activity;
            ActivityContext parentContext;
            if (CorrelationIdContext.TryGetParentContext(out parentContext))
            {
                activity = OpenTelemetryBootstrap.ActivitySource.StartActivity(
                    route,
                    ActivityKind.Server,
                    parentContext);
            }
            else
            {
                activity = OpenTelemetryBootstrap.ActivitySource.StartActivity(
                    route,
                    ActivityKind.Server);
            }

            if (activity == null)
            {
                return;
            }

            var correlationId = CorrelationIdContext.Ensure();
            activity.SetTag(ActivityTags.CorrelationId, correlationId);
            activity.SetTag(ActivityTags.HttpMethod, httpMethod);
            activity.SetTag(ActivityTags.HttpRoute, route);
            activity.SetBaggage(ActivityTags.CorrelationId, correlationId);

            filterContext.HttpContext.Items[ActivityKey] = activity;
            StructuredLoggingBootstrap.EnrichFromActivity(activity);
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext?.HttpContext == null)
            {
                return;
            }

            var stopwatch = filterContext.HttpContext.Items[StopwatchKey] as Stopwatch;
            var activity = filterContext.HttpContext.Items[ActivityKey] as Activity;
            var durationMs = stopwatch != null && stopwatch.IsRunning
                ? stopwatch.ElapsedMilliseconds
                : (stopwatch?.ElapsedMilliseconds ?? 0);

            if (stopwatch != null && stopwatch.IsRunning)
            {
                stopwatch.Stop();
                durationMs = stopwatch.ElapsedMilliseconds;
            }

            var controller = filterContext.ActionDescriptor?.ControllerDescriptor?.ControllerName ?? "unknown";
            var action = filterContext.ActionDescriptor?.ActionName ?? "unknown";
            var route = controller + "." + action;
            var statusCode = filterContext.HttpContext.Response?.StatusCode ?? 0;
            if (filterContext.Exception != null && statusCode < 500)
            {
                statusCode = 500;
            }

            var success = filterContext.Exception == null && statusCode < 500;
            var httpMethod = filterContext.HttpContext.Request?.HttpMethod ?? "GET";

            if (_options.EnableMetrics && _metrics != null)
            {
                _metrics.RecordRequest(route, durationMs, success, httpMethod, statusCode);
                _metrics.RecordMethod("controller", controller, action, durationMs, success);
            }

            if (activity != null)
            {
                activity.SetTag(ActivityTags.HttpStatusCode, statusCode);
                if (filterContext.Exception != null)
                {
                    activity.SetStatus(ActivityStatusCode.Error, filterContext.Exception.Message);
                    activity.AddException(filterContext.Exception);
                }
                else if (!success)
                {
                    activity.SetStatus(ActivityStatusCode.Error, "HTTP " + statusCode);
                }
                else
                {
                    activity.SetStatus(ActivityStatusCode.Ok);
                }

                activity.Dispose();
                filterContext.HttpContext.Items.Remove(ActivityKey);
            }
        }
    }
}
