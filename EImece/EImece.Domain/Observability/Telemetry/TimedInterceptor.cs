using Castle.DynamicProxy;
using EImece.Domain.Observability.Metrics;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.Telemetry
{
    /// <summary>
    /// Castle DynamicProxy interceptor that measures methods marked with <see cref="TimedAttribute"/>.
    /// Thread-safe: no shared mutable state (Stopwatch is per-invocation).
    /// Never throws: telemetry failures are swallowed and written to Debug.
    /// Requires target methods to be <c>virtual</c> (class proxy) or behind an interface (interface proxy).
    /// Metric naming convention:
    ///   Service    → service.{entity}.{operation}  e.g. service.conversations.get_by_user
    ///   Repository → repo.{entity}.{operation}     e.g. repo.conversations.get_by_user
    /// Duration is recorded in milliseconds to OpenTelemetry Histogram via <see cref="Telemetry"/> (Meter: "EImece")
    /// and to the in-memory <see cref="PerfStats"/> store for local visibility.
    /// Also tags <see cref="Activity.Current"/> with timed.metric / timed.duration_ms when present.
    /// Works with sync and async (Task / Task&lt;T&gt;) methods on .NET Framework 4.8.
    /// </summary>
    public sealed class TimedInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            // Fast path: no attribute -> just proceed (no timing overhead).
            var timed = GetTimedAttribute(invocation);
            if (timed == null)
            {
                try
                {
                    invocation.Proceed();
                }
                catch
                {
                    // Business exception must propagate; do not swallow.
                    throw;
                }
                return;
            }

            // Per-invocation stopwatch — thread-safe, no sharing across concurrent calls.
            var stopwatch = Stopwatch.StartNew();

            try
            {
                invocation.Proceed();

                var returnType = invocation.Method.ReturnType;

                // Async: Task / Task<T> — record after the returned Task completes, not just after Proceed().
                if (typeof(Task).IsAssignableFrom(returnType))
                {
                    var task = invocation.ReturnValue as Task;
                    if (task == null)
                    {
                        // Task was null (e.g., async method that synchronously returned null) — record now.
                        stopwatch.Stop();
                        Record(timed, stopwatch, invocation);
                        return;
                    }

                    if (returnType == typeof(Task))
                    {
                        // Non-generic Task
                        invocation.ReturnValue = InterceptAsync(task, timed, stopwatch, invocation);
                    }
                    else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        // Task<T> — use reflection to create generic handler.
                        var resultType = returnType.GetGenericArguments()[0];
                        var method = typeof(TimedInterceptor).GetMethod(
                            nameof(InterceptAsyncGeneric),
                            BindingFlags.NonPublic | BindingFlags.Static);
                        var generic = method.MakeGenericMethod(resultType);
                        invocation.ReturnValue = generic.Invoke(null, new object[] { task, timed, stopwatch, invocation });
                    }
                    else
                    {
                        // Unexpected Task subtype — treat as sync.
                        stopwatch.Stop();
                        Record(timed, stopwatch, invocation);
                    }

                    // For async paths, recording happens in the continuation; return now.
                    return;
                }

                // Sync path: Proceed() already completed.
                stopwatch.Stop();
                Record(timed, stopwatch, invocation);
            }
            catch (Exception)
            {
                // Synchronous exception from Proceed() — still record duration before rethrowing.
                try
                {
                    if (stopwatch.IsRunning)
                        stopwatch.Stop();
                    Record(timed, stopwatch, invocation);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"TimedInterceptor record failed for '{timed?.Name ?? "unknown"}': {ex}");
                }

                throw;
            }
        }

        // Non-generic Task continuation.
        private static async Task InterceptAsync(Task task, TimedAttribute timed, Stopwatch stopwatch, IInvocation invocation)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            finally
            {
                // Ensure stopwatch stopped exactly once, regardless of success/fault/cancel.
                if (stopwatch.IsRunning)
                    stopwatch.Stop();
                Record(timed, stopwatch, invocation);
            }
        }

        // Generic Task<T> continuation. Must remain non-public for reflection lookup.
        private static async Task<T> InterceptAsyncGeneric<T>(Task<T> task, TimedAttribute timed, Stopwatch stopwatch, IInvocation invocation)
        {
            try
            {
                // Await and capture result; ConfigureAwait(false) avoids deadlocks on ASP.NET (.NET Framework).
                var result = await task.ConfigureAwait(false);
                return result;
            }
            finally
            {
                if (stopwatch.IsRunning)
                    stopwatch.Stop();
                Record(timed, stopwatch, invocation);
            }
        }

        // Central recording: OTel histogram + PerfStats + Activity tags. Never throws.
        private static void Record(TimedAttribute timed, Stopwatch stopwatch, IInvocation invocation)
        {
            try
            {
                var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

                // 1. OTel Histogram (Meter: "EImece", cached via Telemetry — thread-safe).
                var histogram = Telemetry.GetOrCreateHistogram(timed.Name, timed.Description);
                histogram.Record(elapsedMs);

                // 2. In-memory PerfStats store (1-day retention).
                PerfStats.Record(timed.Name, elapsedMs);

                // 3. Enrich current Activity if any (created by AspNet instrumentation or TelemetryActionFilter).
                var activity = Activity.Current;
                if (activity != null)
                {
                    activity.SetTag("timed.metric", timed.Name);
                    activity.SetTag("timed.duration_ms", elapsedMs);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TimedInterceptor failed to record metric '{timed?.Name ?? "unknown"}': {ex}");
            }
        }

        // Best-effort attribute lookup preferring the concrete target method.
        private static TimedAttribute GetTimedAttribute(IInvocation invocation)
        {
            try
            {
                // MethodInvocationTarget is the concrete implementation; Method is the proxy/interface method.
                var targetMethod = invocation.MethodInvocationTarget;
                if (targetMethod != null)
                {
                    var attr = (TimedAttribute)Attribute.GetCustomAttribute(targetMethod, typeof(TimedAttribute), true);
                    if (attr != null)
                        return attr;
                }

                var method = invocation.Method;
                if (method != null && method != targetMethod)
                {
                    var attr = (TimedAttribute)Attribute.GetCustomAttribute(method, typeof(TimedAttribute), true);
                    if (attr != null)
                        return attr;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TimedInterceptor attribute lookup failed: {ex}");
                return null;
            }
        }
    }
}
