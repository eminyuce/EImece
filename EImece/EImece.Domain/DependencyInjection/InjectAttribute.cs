using System;

namespace EImece.Domain.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class InjectAttribute : Attribute
    {
    }
}
