using EImece.Domain.Observability.Telemetry;
using System;
using System.Diagnostics;
using System.Web.Mvc;

namespace EImece.Filters
{
    /// <summary>
    /// Business-metric filter for ASP.NET MVC 5 / .NET Framework 4.8.
    /// Records action duration in milliseconds to an OpenTelemetry Histogram.
    /// Overall HTTP request duration is already covered by OpenTelemetry.Instrumentation.AspNet;
    /// this filter adds a per-action business metric.
    /// When used without arguments (e.g. [TimedActionFilter] on BaseController) the metric name is
    /// auto-derived as "app.{controller}.{action}" so every storefront action is measured
    /// without manual per-method decoration. Pass an explicit name for a custom business metric.
    /// Safe for concurrent requests — Stopwatch is stored per-request in HttpContext.Items,
    /// not in a shared instance field (filter attributes are cached and shared across requests).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class TimedActionFilterAttribute : ActionFilterAttribute
    {
        private readonly string _name;
        private readonly string _description;

        /// <summary>
        /// Creates a Timed filter.
        /// </summary>
        /// <param name="name">Histogram / metric name, e.g. "service.conversations.getConversations". When null/empty, auto-derived as "app.{controller}.{action}".</param>
        /// <param name="description">Optional description exported to OTLP / Azure Monitor.</param>
        public TimedActionFilterAttribute(string name = null, string description = null)
        {
            // Allow null for auto-derived name (used on BaseController to cover all actions).
            // Explicit empty string is treated as invalid.
            if (name != null && string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Timed metric name must not be empty.", nameof(name));
            }

            _name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            _description = description;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext?.HttpContext != null)
            {
                var effectiveName = GetEffectiveName(filterContext);
                var key = GetItemKey(effectiveName);
                filterContext.HttpContext.Items[key] = Stopwatch.StartNew();
                // Store effective name alongside so OnActionExecuted can retrieve it even if
                // ActionDescriptor differs slightly after execution (e.g. exception filters).
                filterContext.HttpContext.Items[key + "_name"] = effectiveName;
            }

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                if (filterContext?.HttpContext != null)
                {
                    // Re-derive name; fallback to stored name if available.
                    var derivedName = GetEffectiveName(filterContext);
                    var key = GetItemKey(derivedName);
                    var storedName = filterContext.HttpContext.Items[key + "_name"] as string;
                    var effectiveName = !string.IsNullOrWhiteSpace(storedName) ? storedName : derivedName;

                    var stopwatch = filterContext.HttpContext.Items[key] as Stopwatch;
                    // Clean up both entries early.
                    filterContext.HttpContext.Items.Remove(key);
                    filterContext.HttpContext.Items.Remove(key + "_name");

                    if (stopwatch != null)
                    {
                        if (stopwatch.IsRunning)
                        {
                            stopwatch.Stop();
                        }

                        var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

                        // Record to OTel Histogram (ms). Cached via Telemetry helper.
                        var histogram = Telemetry.GetOrCreateHistogram(effectiveName, _description);
                        histogram.Record(elapsedMs);

                        // Annotate the current Activity (created by TelemetryActionFilter
                        // or AspNet instrumentation) so traces show business duration.
                        var activity = Activity.Current;
                        if (activity != null)
                        {
                            activity.SetTag("timed.metric", effectiveName);
                            activity.SetTag("timed.duration_ms", elapsedMs);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Never throw from telemetry — swallow and log to debug.
                var logName = _name ?? "(auto)";
                Debug.WriteLine("TimedActionFilterAttribute failed to record metric '" + logName + "': " + ex);
            }
            finally
            {
                base.OnActionExecuted(filterContext);
            }
        }

        private string GetEffectiveName(ActionExecutingContext ctx)
        {
            if (!string.IsNullOrWhiteSpace(_name))
            {
                return _name;
            }

            var controller = ctx.ActionDescriptor?.ControllerDescriptor?.ControllerName ?? "unknown";
            var action = ctx.ActionDescriptor?.ActionName ?? "unknown";
            return $"app.{controller}.{action}".ToLowerInvariant();
        }

        private string GetEffectiveName(ActionExecutedContext ctx)
        {
            if (!string.IsNullOrWhiteSpace(_name))
            {
                return _name;
            }

            var controller = ctx.ActionDescriptor?.ControllerDescriptor?.ControllerName ?? "unknown";
            var action = ctx.ActionDescriptor?.ActionName ?? "unknown";
            return $"app.{controller}.{action}".ToLowerInvariant();
        }

        private static string GetItemKey(string effectiveName)
        {
            return "__Timed_Stopwatch_" + effectiveName;
        }
    }
}
