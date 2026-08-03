using System;

namespace EImece.Domain.DependencyInjection
{
    /// <summary>
    /// Marks a property for injection by the MS.DI composition root.
    /// Replaces Ninject's <c>[Inject]</c> attribute without requiring the Ninject package.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class InjectAttribute : Attribute
    {
    }
}
