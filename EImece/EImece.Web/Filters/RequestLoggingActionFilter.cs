using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Logging;
using EImece.Web.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Web.Mvc;

namespace EImece.Web.Filters
{
    public sealed class RequestLoggingActionFilter : IActionFilter
    {
        private readonly ObservabilityOptions _options;
        private readonly ILogger<RequestLoggingActionFilter> _logger;
        private const string StopwatchKey = "RequestLoggingStopwatch";

        public RequestLoggingActionFilter(ObservabilityOptions options, ILogger<RequestLoggingActionFilter> logger)
        {
            _options = options ?? ObservabilityOptions.FromAppConfig();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!_options.EnableRequestLogging)
            {
                return;
            }

            filterContext.HttpContext.Items[StopwatchKey] = System.Diagnostics.Stopwatch.StartNew();
            WebLoggingHelper.EnrichFromHttpContext();
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

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                using (WebLoggingHelper.BeginRequestScope(_logger))
                {
                    StructuredLoggingBootstrap.LogRequestCompleted(
                        stopwatch.ElapsedMilliseconds,
                        filterContext.HttpContext.Response.StatusCode,
                        filterContext.HttpContext.Request.HttpMethod,
                        filterContext.HttpContext.Request.Url?.AbsolutePath,
                        filterContext.HttpContext.User?.Identity?.IsAuthenticated == true
                            ? filterContext.HttpContext.User.Identity.Name
                            : null);
                }
            }

            if (filterContext.Exception != null)
            {
                StructuredLoggingBootstrap.LogException(filterContext.Exception, "Unhandled MVC action exception");
            }
        }
    }
}
