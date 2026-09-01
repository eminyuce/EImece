using EImece.Domain.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EImece.App_Start
{
    /// <summary>
    /// MVC <see cref="IDependencyResolver"/> backed by Microsoft.Extensions.DependencyInjection.
    /// Resolves from the current HTTP request scope when available; otherwise from the root provider.
    /// Unregistered concrete types (e.g. controllers) are activated via ActivatorUtilities + property injection.
    /// </summary>
    public sealed class MsDiDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _rootProvider;

        public MsDiDependencyResolver(IServiceProvider rootProvider)
        {
            _rootProvider = rootProvider ?? throw new ArgumentNullException(nameof(rootProvider));
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                return null;
            }

            // Controllers need a request scope for scoped services (validateScopes: true).
            DependencyInjectionConfig.BeginRequestScope();
            var provider = DependencyInjectionConfig.GetRequestServiceProvider() ?? _rootProvider;
            try
            {
                var service = provider.GetService(serviceType);
                if (service != null)
                {
                    // Registered services already receive property injection from their factories.
                    return service;
                }
            }
            catch (InvalidOperationException)
            {
                // Root provider cannot resolve scoped service outside active request scope
            }

            if (serviceType.IsClass && !serviceType.IsAbstract && !serviceType.IsInterface)
            {
                // Controllers and other unregistered concretes: ctor + [Inject] property injection.
                // Do not swallow failures — returning null makes MVC Activator.CreateInstance
                // a controller with null [Inject] properties (NullReferenceException later).
                return PropertyInjector.Create(serviceType, provider);
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (serviceType == null)
            {
                return Enumerable.Empty<object>();
            }

            var provider = DependencyInjectionConfig.GetRequestServiceProvider() ?? _rootProvider;
            return provider.GetServices(serviceType);
        }
    }
}
