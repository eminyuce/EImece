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
                var dependency = provider.GetService(property.PropertyType);
                if (dependency != null)
                {
                    property.SetValue(instance, dependency, null);
                }
            }

            foreach (var field in GetInjectFields(instance.GetType()))
            {
                var dependency = provider.GetService(field.FieldType);
                if (dependency != null)
                {
                    field.SetValue(instance, dependency);
                }
            }
        }

        private static PropertyInfo[] GetInjectProperties(Type type)
        {
            return InjectPropertiesCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0
                                && p.IsDefined(typeof(InjectAttribute), inherit: true))
                    .ToArray());
        }

        private static readonly ConcurrentDictionary<Type, FieldInfo[]> InjectFieldsCache =
            new ConcurrentDictionary<Type, FieldInfo[]>();

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
