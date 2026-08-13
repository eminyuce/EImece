using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace EImece.Domain.DependencyInjection
{
    /// <summary>
    /// Creates instances and populates properties/fields marked with <see cref="InjectAttribute"/>,
    /// preserving the Ninject-style property injection used throughout this codebase.
    /// Tracks in-flight constructions so circular property/constructor dependencies resolve
    /// to the same in-scope instance (avoids MS.DI "same key has already been added").
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

            // Register before running the constructor so re-entrant resolves of this type
            // (via interface or concrete) do not re-enter MS.DI's scope cache.
            var instance = FormatterServices.GetUninitializedObject(implementationType);
            tracker[implementationType] = instance;
            try
            {
                InvokeConstructor(instance, implementationType, provider);
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
                var dependency = ResolveRequired(provider, property.PropertyType, instance.GetType(), property.Name);
                property.SetValue(instance, dependency, null);
            }

            foreach (var field in GetInjectFields(instance.GetType()))
            {
                var dependency = ResolveRequired(provider, field.FieldType, instance.GetType(), field.Name);
                field.SetValue(instance, dependency);
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
            foreach (var kvp in tracker.Where(pair => serviceType.IsAssignableFrom(pair.Key)))
            {
                return kvp.Value;
            }

            return null;
        }

        /// <summary>
        /// Resolves a dependency for property/field/constructor injection.
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

        private static object ResolveRequired(
            IServiceProvider provider,
            Type serviceType,
            Type targetType,
            string memberName)
        {
            var dependency = Resolve(provider, serviceType);
            if (dependency != null)
            {
                return dependency;
            }

            // GetService returns null for missing registrations; prefer a clear error for [Inject].
            try
            {
                return provider.GetRequiredService(serviceType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to inject {serviceType.FullName} into {targetType.FullName}.{memberName}.",
                    ex);
            }
        }

        private static void InvokeConstructor(object instance, Type implementationType, IServiceProvider provider)
        {
            var ctor = SelectConstructor(implementationType);
            if (ctor == null)
            {
                return;
            }

            var parameters = ctor.GetParameters();
            if (parameters.Length == 0)
            {
                ctor.Invoke(instance, null);
                return;
            }

            var args = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.HasDefaultValue && !IsInjectableServiceType(parameter.ParameterType))
                {
                    args[i] = parameter.DefaultValue;
                    continue;
                }

                var arg = Resolve(provider, parameter.ParameterType);
                if (arg == null && parameter.HasDefaultValue)
                {
                    arg = parameter.DefaultValue;
                }
                else if (arg == null)
                {
                    arg = provider.GetRequiredService(parameter.ParameterType);
                }

                args[i] = arg;
            }

            ctor.Invoke(instance, args);
        }

        /// <summary>
        /// Picks the greediest public constructor whose parameters are DI services or optional,
        /// matching <see cref="ActivatorUtilities"/> (skips ctors with bool/string/primitive deps).
        /// </summary>
        private static ConstructorInfo SelectConstructor(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            if (constructors.Length == 0)
            {
                return null;
            }

            var applicable = constructors
                .Where(c => c.GetParameters().All(p =>
                    p.HasDefaultValue || IsInjectableServiceType(p.ParameterType)))
                .OrderByDescending(c => c.GetParameters().Length)
                .ToList();

            if (applicable.Count > 0)
            {
                return applicable[0];
            }

            return constructors.FirstOrDefault(c => c.GetParameters().Length == 0)
                   ?? constructors.OrderBy(c => c.GetParameters().Length).First();
        }

        private static bool IsInjectableServiceType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (type == typeof(string) || type.IsPrimitive || type.IsEnum)
            {
                return false;
            }

            if (type == typeof(decimal) || type == typeof(DateTime) || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan) || type == typeof(Guid))
            {
                return false;
            }

            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
            {
                return IsInjectableServiceType(underlying);
            }

            return type.IsInterface || type.IsClass;
        }

        private static PropertyInfo[] GetInjectProperties(Type type)
        {
            return InjectPropertiesCache.GetOrAdd(type, t =>
            {
                // Walk DeclaredOnly per type: GetProperties(NonPublic) omits some base members.
                var props = new List<PropertyInfo>();
                for (var current = t; current != null && current != typeof(object); current = current.BaseType)
                {
                    // NonPublic is required: [Inject] may be on private/protected properties, including on base types.
#pragma warning disable S3011 // Reflection must reach non-public [Inject] members on base types
                    props.AddRange(
                        current.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                            .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0
                                        && p.IsDefined(typeof(InjectAttribute), inherit: true)));
#pragma warning restore S3011
                }

                return props.ToArray();
            });
        }

        private static FieldInfo[] GetInjectFields(Type type)
        {
            return InjectFieldsCache.GetOrAdd(type, t =>
            {
                var fields = new List<FieldInfo>();
                for (var current = t; current != null && current != typeof(object); current = current.BaseType)
                {
                    // NonPublic is required: [Inject] may be on private/protected fields, including on base types.
#pragma warning disable S3011 // Reflection must reach non-public [Inject] members on base types
                    fields.AddRange(
                        current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                            .Where(f => !f.IsInitOnly && !f.IsLiteral
                                        && f.IsDefined(typeof(InjectAttribute), inherit: true)));
#pragma warning restore S3011
                }

                return fields.ToArray();
            });
        }
    }
}
