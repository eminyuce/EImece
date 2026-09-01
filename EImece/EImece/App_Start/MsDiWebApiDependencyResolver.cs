using EImece.Domain.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace EImece.App_Start
{
    /// <summary>
    /// Web API dependency resolver that shares the same per-request MS.DI scope as MVC.
    /// </summary>
    public sealed class MsDiWebApiDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _rootProvider;

        public MsDiWebApiDependencyResolver(IServiceProvider rootProvider)
        {
            _rootProvider = rootProvider ?? throw new ArgumentNullException(nameof(rootProvider));
        }

        public IDependencyScope BeginScope()
        {
            // Prefer the request scope created in Application_BeginRequest so MVC and Web API share it.
            var requestProvider = DependencyInjectionConfig.GetRequestServiceProvider();
            if (requestProvider != null)
            {
                return new MsDiWebApiDependencyScope(requestProvider, ownsScope: false);
            }

            var scope = _rootProvider.CreateScope();
            return new MsDiWebApiDependencyScope(scope.ServiceProvider, ownsScope: true, scope);
        }

        public object GetService(Type serviceType)
        {
            return Resolve(DependencyInjectionConfig.GetRequestServiceProvider() ?? _rootProvider, serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var provider = DependencyInjectionConfig.GetRequestServiceProvider() ?? _rootProvider;
            return provider.GetServices(serviceType);
        }

        public void Dispose()
        {
            // Root provider lifetime is owned by DependencyInjectionConfig.
        }

        private static object Resolve(IServiceProvider provider, Type serviceType)
        {
            if (serviceType == null)
            {
                return null;
            }

            var service = provider.GetService(serviceType);
            if (service != null)
            {
                // Registered services already receive property injection from their factories.
                return service;
            }

            if (serviceType.IsClass && !serviceType.IsAbstract && !serviceType.IsInterface)
            {
                try
                {
                    return PropertyInjector.Create(serviceType, provider);
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }

            return null;
        }

        private sealed class MsDiWebApiDependencyScope : IDependencyScope
        {
            private readonly IServiceProvider _provider;
            private readonly bool _ownsScope;
            private readonly IServiceScope _scope;

            public MsDiWebApiDependencyScope(IServiceProvider provider, bool ownsScope, IServiceScope scope = null)
            {
                _provider = provider;
                _ownsScope = ownsScope;
                _scope = scope;
            }

            public object GetService(Type serviceType)
            {
                return Resolve(_provider, serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                return _provider.GetServices(serviceType);
            }

            public void Dispose()
            {
                if (_ownsScope)
                {
                    _scope?.Dispose();
                }
            }
        }
    }
}
