using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace EImece.Domain.DependencyInjection
{
    /// <summary>
    /// Creates instances via <see cref="ActivatorUtilities"/> and populates
    /// properties marked with <see cref="InjectAttribute"/>, preserving the
    /// Ninject-style property injection used throughout this codebase.
    /// Tracks in-flight constructions so circular property dependencies resolve
    /// to the same in-scope instance (same behavior as Ninject request scope).
    /// </summary>
    public static class PropertyInjector
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> InjectPropertiesCache =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static readonly ConcurrentDictionary<Type, FieldInfo[]> InjectFieldsCache =
            new ConcurrentDictionary<Type, FieldInfo[]>();

        private static readonly AsyncLocal<Dictionary<Type, object>> UnderConstruction =
            new AsyncLocal<Dictionary<Type, object>>();

        public static T Create<T>(IServiceProvider provider)
        {
            return (T)Create(typeof(T), provider);
        }

        public static object Create(Type implementationType, IServiceProvider provider)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            var tracker = UnderConstruction.Value;
            if (tracker == null)
            {
                tracker = new Dictionary<Type, object>();
                UnderConstruction.Value = tracker;
            }

            if (tracker.TryGetValue(implementationType, out var existing))
            {
                return existing;
            }

            var instance = ActivatorUtilities.CreateInstance(provider, implementationType);
            tracker[implementationType] = instance;
            try
            {
                Inject(instance, provider);
            }
            finally
            {
                tracker.Remove(implementationType);
            }

            return instance;
        }

        public static void Inject(object instance, IServiceProvider provider)
        {
            if (instance == null || provider == null)
            {
                return;
            }

            foreach (var property in GetInjectProperties(instance.GetType()))
            {
                var dependency = Resolve(provider, property.PropertyType);
                if (dependency != null)
                {
                    property.SetValue(instance, dependency, null);
                }
            }

            foreach (var field in GetInjectFields(instance.GetType()))
            {
                var dependency = Resolve(provider, field.FieldType);
                if (dependency != null)
                {
                    field.SetValue(instance, dependency);
                }
            }
        }

        /// <summary>
        /// Returns an in-flight instance assignable to <paramref name="serviceType"/>, if any.
        /// Used by DI factories and property injection to break cycles without re-entering MS.DI
        /// (which throws "An item with the same key has already been added" from CallSiteRuntimeResolver).
        /// </summary>
        public static object TryGetUnderConstruction(Type serviceType)
        {
            if (serviceType == null)
            {
                return null;
            }

            var tracker = UnderConstruction.Value;
            if (tracker == null || tracker.Count == 0)
            {
                return null;
            }

            if (tracker.TryGetValue(serviceType, out var exact))
            {
                return exact;
            }

            // Interface / base-type [Inject] properties map to the concrete being built.
            foreach (var kvp in tracker)
            {
                if (serviceType.IsAssignableFrom(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves a dependency for property/field injection.
        /// Must short-circuit to in-flight instances before calling MS.DI.
        /// </summary>
        private static object Resolve(IServiceProvider provider, Type serviceType)
        {
            var underConstruction = TryGetUnderConstruction(serviceType);
            if (underConstruction != null)
            {
                return underConstruction;
            }

            return provider.GetService(serviceType);
        }

        private static PropertyInfo[] GetInjectProperties(Type type)
        {
            return InjectPropertiesCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0
                                && p.IsDefined(typeof(InjectAttribute), inherit: true))
                    .ToArray());
        }

        private static FieldInfo[] GetInjectFields(Type type)
        {
            return InjectFieldsCache.GetOrAdd(type, t =>
                t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => !f.IsInitOnly && !f.IsLiteral
                                && f.IsDefined(typeof(InjectAttribute), inherit: true))
                    .ToArray());
        }
    }
}
