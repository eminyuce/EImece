using System;

namespace EImece.Domain.Observability.Telemetry
{
    /// <summary>
    /// Plain marker attribute for service / repository methods measured by <see cref="TimedInterceptor"/>.
    /// NOT an MVC ActionFilter — use <c>EImece.Filters.TimedActionFilterAttribute</c> for controllers.
    /// Requires the target method to be <c>virtual</c> so Castle DynamicProxy can intercept.
    /// Naming convention:
    ///   Service    -> service.{entity}.{operation}   e.g. service.conversations.get_by_user
    ///   Repository -> repo.{entity}.{operation}      e.g. repo.conversations.get_by_user
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class TimedAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public TimedAttribute(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Metric name is required.", nameof(name));

            Name = name.Trim();
            Description = description;
        }
    }
}
