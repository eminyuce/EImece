using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading.Tasks;

namespace EImece.Tests.Infrastructure
{
    /// <summary>
    /// Routes interface calls to a plain store object by method name and arity.
    /// Used to isolate service-layer business rules from Entity Framework.
    /// </summary>
    internal sealed class FakeServiceProxy<TInterface> : RealProxy where TInterface : class
    {
        private readonly object _target;

        public FakeServiceProxy(object target) : base(typeof(TInterface))
        {
            _target = target;
        }

        public override IMessage Invoke(IMessage msg)
        {
            var call = (IMethodCallMessage)msg;
            try
            {
                var methods = _target.GetType().GetMethods()
                    .Where(m => m.Name == call.MethodName && m.GetParameters().Length == call.Args.Length)
                    .ToList();
                if (methods.Count > 0)
                {
                    var result = methods[0].Invoke(_target, call.Args);
                    return new ReturnMessage(result, null, 0, call.LogicalCallContext, call);
                }
            }
            catch (TargetInvocationException ex)
            {
                return new ReturnMessage(ex.InnerException, call);
            }

            object defaultResult = null;
            if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
            {
                if (mi.ReturnType == typeof(Task))
                {
                    defaultResult = Task.CompletedTask;
                }
                else if (mi.ReturnType.IsGenericType && mi.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var innerType = mi.ReturnType.GetGenericArguments()[0];
                    var defaultInner = innerType.IsValueType ? Activator.CreateInstance(innerType) : null;
                    defaultResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(innerType)
                        .Invoke(null, new[] { defaultInner });
                }
                else if (mi.ReturnType.IsValueType)
                {
                    defaultResult = Activator.CreateInstance(mi.ReturnType);
                }
            }

            return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
        }

        public TInterface Instance
        {
            get { return (TInterface)GetTransparentProxy(); }
        }
    }
}
