using EImece.Web.Configuration;
using Microsoft.Extensions.Options;

namespace EImece.Web.DependencyInjection;

/// <summary>
/// Composition-root helpers for the ASP.NET Core host.
/// Preserves Microsoft.Extensions.DependencyInjection (no third-party IoC).
/// Iyzico/Smtp/Media Options are registered in AddEImeceInfrastructure (Domain.Core).
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEImeceCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EImeceOptions>(configuration.GetSection(EImeceOptions.SectionName));

        // Validate options early in Development to catch missing config.
        services.AddSingleton<IValidateOptions<EImeceOptions>, ValidateEImeceOptions>();

        return services;
    }
}

internal sealed class ValidateEImeceOptions : IValidateOptions<EImeceOptions>
{
    public ValidateOptionsResult Validate(string? name, EImeceOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Domain))
        {
            return ValidateOptionsResult.Fail("EImece:Domain must be configured.");
        }

        return ValidateOptionsResult.Success;
    }
}
