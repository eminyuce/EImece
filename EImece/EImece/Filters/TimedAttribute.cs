using EImece.Domain.Observability.Telemetry;
using System;
using System.Diagnostics;
using System.Web.Mvc;

namespace EImece.Filters
{
    /// <summary>
    /// Opt-in business-metric filter for ASP.NET MVC 5 / .NET Framework 4.8.
    /// Records action duration in milliseconds to an OpenTelemetry Histogram.
    /// Overall HTTP request duration is already covered by OpenTelemetry.Instrumentation.AspNet;
    /// apply [Timed] only where you want a distinct business-oriented metric name.
    /// Safe for concurrent requests — Stopwatch is stored per-request in HttpContext.Items,
    /// not in a shared instance field (filter attributes are cached and shared across requests).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class TimedAttribute : ActionFilterAttribute
    {
        private readonly string _name;
        private readonly string _description;

        // Per-request key. Includes metric name so two [Timed] attributes with different names
        // on the same request (e.g. class + method) do not collide.
        private string ItemKey => "__Timed_Stopwatch_" + _name;

        /// <summary>
        /// Creates a Timed filter.
        /// </summary>
        /// <param name="name">Histogram / metric name, e.g. "service.conversations.getConversations". Must be non-empty.</param>
        /// <param name="description">Optional description exported to OTLP / Azure Monitor.</param>
        public TimedAttribute(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Timed metric name must not be empty.", nameof(name));
            }

            _name = name.Trim();
            _description = description;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Store per-request Stopwatch in HttpContext.Items — never in an instance field.
            if (filterContext?.HttpContext != null)
            {
                filterContext.HttpContext.Items[ItemKey] = Stopwatch.StartNew();
            }

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                if (filterContext?.HttpContext != null)
                {
                    var stopwatch = filterContext.HttpContext.Items[ItemKey] as Stopwatch;
                    if (stopwatch != null)
                    {
                        if (stopwatch.IsRunning)
                        {
                            stopwatch.Stop();
                        }

                        var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

                        // Clean up to avoid leaking into later pipeline stages.
                        filterContext.HttpContext.Items.Remove(ItemKey);

                        // Record to OTel Histogram (ms). Cached via Telemetry helper.
                        var histogram = Telemetry.GetOrCreateHistogram(_name, _description);
                        histogram.Record(elapsedMs);

                        // Optional: also annotate the current Activity (created by TelemetryActionFilter
                        // or AspNet instrumentation) so traces show business duration.
                        var activity = Activity.Current;
                        if (activity != null)
                        {
                            activity.SetTag("timed.metric", _name);
                            activity.SetTag("timed.duration_ms", elapsedMs);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Never throw from telemetry — swallow and log to debug.
                Debug.WriteLine("TimedAttribute failed to record metric '" + _name + "': " + ex);
            }
            finally
            {
                base.OnActionExecuted(filterContext);
            }
        }
    }
}
