using EImece.Domain.Observability.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace EImece.Domain.Configuration
{
    public static class EimeceHealthCheckRegistration
    {
        public static IServiceCollection AddEimeceHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<SqlServerHealthCheck>(SqlServerHealthCheck.DefaultName)
                .AddCheck<FileStorageHealthCheck>(FileStorageHealthCheck.DefaultName)
                .AddCheck<BackgroundServiceHealthCheck>(BackgroundServiceHealthCheck.DefaultName)
                .AddCheck<ExternalApiHealthCheck>(ExternalApiHealthCheck.DefaultName);

            return services;
        }
    }
}
