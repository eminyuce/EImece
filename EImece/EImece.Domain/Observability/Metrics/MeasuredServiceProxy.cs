using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.Metrics
{
    /// <summary>
    /// Transparent proxy that times every interface method on a Service (sync and async)
    /// and records P90/P95/P99-capable latency samples via <see cref="IApplicationMetrics"/>.
    /// Uses <see cref="RealProxy"/> so no extra NuGet package is required on .NET Framework 4.8.1.
    /// </summary>
    public sealed class MeasuredServiceProxy : RealProxy
    {
        private static readonly MethodInfo WrapTaskOfTDefinition =
            typeof(MeasuredServiceProxy).GetMethod(nameof(WrapTaskOfT), BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly object _target;
        private readonly IApplicationMetrics _metrics;
        private readonly string _typeName;

        private MeasuredServiceProxy(Type serviceType, object target, IApplicationMetrics metrics)
            : base(serviceType)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            _typeName = serviceType.Name;
        }

        /// <summary>
        /// Returns a transparent proxy for <typeparamref name="TService"/> that records method latency.
        /// When <paramref name="metrics"/> is null, returns the original target unchanged.
        /// </summary>
        public static TService Create<TService>(TService target, IApplicationMetrics metrics)
            where TService : class
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (metrics == null)
            {
                return target;
            }

            var serviceType = typeof(TService);
            if (!serviceType.IsInterface)
            {
                throw new ArgumentException("MeasuredServiceProxy requires an interface service type.", nameof(target));
            }

            // Avoid double-wrapping.
            if (RemotingServices.IsTransparentProxy(target))
            {
                return target;
            }

            var proxy = new MeasuredServiceProxy(serviceType, target, metrics);
            return (TService)proxy.GetTransparentProxy();
        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = (IMethodCallMessage)msg;
            var method = (MethodInfo)methodCall.MethodBase;

            if (ShouldSkip(method))
            {
                return InvokeTargetPassthrough(methodCall, method);
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = method.Invoke(_target, methodCall.Args);

                if (result is Task task)
                {
                    var wrapped = WrapTaskResult(method, task, stopwatch);
                    return new ReturnMessage(wrapped, null, 0, methodCall.LogicalCallContext, methodCall);
                }

                stopwatch.Stop();
                Record(method.Name, stopwatch.ElapsedMilliseconds, success: true);
                return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            catch (TargetInvocationException ex)
            {
                stopwatch.Stop();
                Record(method.Name, stopwatch.ElapsedMilliseconds, success: false);
                var fault = ex.InnerException ?? ex;
                return new ReturnMessage(fault, methodCall);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Record(method.Name, stopwatch.ElapsedMilliseconds, success: false);
                return new ReturnMessage(ex, methodCall);
            }
        }

        private IMessage InvokeTargetPassthrough(IMethodCallMessage methodCall, MethodInfo method)
        {
            try
            {
                var result = method.Invoke(_target, methodCall.Args);
                return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            catch (TargetInvocationException ex)
            {
                return new ReturnMessage(ex.InnerException ?? ex, methodCall);
            }
        }

        private object WrapTaskResult(MethodInfo method, Task task, Stopwatch stopwatch)
        {
            var returnType = method.ReturnType;
            if (returnType == typeof(Task))
            {
                return WrapTask(task, method.Name, stopwatch);
            }

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultType = returnType.GetGenericArguments()[0];
                var generic = WrapTaskOfTDefinition.MakeGenericMethod(resultType);
                return generic.Invoke(this, new object[] { task, method.Name, stopwatch });
            }

            // ValueTask / other awaitables are uncommon in this codebase; treat as sync completion.
            stopwatch.Stop();
            Record(method.Name, stopwatch.ElapsedMilliseconds, !task.IsFaulted && !task.IsCanceled);
            return task;
        }

        private async Task WrapTask(Task task, string methodName, Stopwatch stopwatch)
        {
            try
            {
                await task.ConfigureAwait(false);
                stopwatch.Stop();
                Record(methodName, stopwatch.ElapsedMilliseconds, success: true);
            }
            catch
            {
                stopwatch.Stop();
                Record(methodName, stopwatch.ElapsedMilliseconds, success: false);
                throw;
            }
        }

        private async Task<T> WrapTaskOfT<T>(Task task, string methodName, Stopwatch stopwatch)
        {
            try
            {
                var typed = (Task<T>)task;
                var result = await typed.ConfigureAwait(false);
                stopwatch.Stop();
                Record(methodName, stopwatch.ElapsedMilliseconds, success: true);
                return result;
            }
            catch
            {
                stopwatch.Stop();
                Record(methodName, stopwatch.ElapsedMilliseconds, success: false);
                throw;
            }
        }

        private void Record(string methodName, long durationMs, bool success)
        {
            _metrics.RecordMethod("service", _typeName, methodName, durationMs, success);
        }

        private static bool ShouldSkip(MethodInfo method)
        {
            if (method == null)
            {
                return true;
            }

            // Skip object infrastructure and property accessors (noise / not business latency).
            if (method.DeclaringType == typeof(object))
            {
                return true;
            }

            if (method.IsSpecialName
                && (method.Name.StartsWith("get_", StringComparison.Ordinal)
                    || method.Name.StartsWith("set_", StringComparison.Ordinal)))
            {
                return true;
            }

            return false;
        }
    }
}
