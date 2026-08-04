using EImece.Domain.Core.Caching;
using EImece.Domain.Core.Configuration;
using EImece.Domain.Core.Hosting;
using EImece.Domain.Core.Http;
using EImece.Domain.Core.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace EImece.Domain.Core.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers Phase 4 infrastructure: Options, cache, media, resilient HttpClient, scheduler hosted service.
    /// Logging (NLog) is configured on the Web host.
    /// </summary>
    public static IServiceCollection AddEImeceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.Configure<CaptchaOptions>(configuration.GetSection(CaptchaOptions.SectionName));
        services.Configure<MediaOptions>(configuration.GetSection(MediaOptions.SectionName));
        services.Configure<ObservabilityOptions>(configuration.GetSection(ObservabilityOptions.SectionName));
        services.Configure<HttpClientResilienceOptions>(configuration.GetSection(HttpClientResilienceOptions.SectionName));
        services.Configure<QuartzOptions>(configuration.GetSection(QuartzOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        // Keep Quartz flag on EImece section in sync if present (Phase 2 key).
        var legacyQuartz = configuration.GetValue<bool?>("EImece:QuartzSchedulerIsEnabled");
        if (legacyQuartz.HasValue)
        {
            services.PostConfigure<QuartzOptions>(o => o.IsEnabled = legacyQuartz.Value || o.IsEnabled);
        }

        services.AddMemoryCache();
        services.AddSingleton<IEimeceCacheProvider, MemoryCacheProvider>();
        services.AddSingleton<IMediaFileService, MediaFileService>();

        var httpOptions = configuration.GetSection(HttpClientResilienceOptions.SectionName).Get<HttpClientResilienceOptions>()
            ?? new HttpClientResilienceOptions();

        services.AddHttpClient(ResilientHttpClient.HttpClientName)
            .AddStandardResilienceHandler(options =>
            {
                // Standard resilience requires SamplingDuration >= 2 × AttemptTimeout.
                var attemptSeconds = Math.Max(5, httpOptions.TimeoutSeconds / 2);
                var samplingSeconds = Math.Max(attemptSeconds * 2, httpOptions.TimeoutSeconds);
                var totalSeconds = Math.Max(samplingSeconds, httpOptions.TimeoutSeconds * Math.Max(1, httpOptions.RetryCount));

                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(attemptSeconds);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(totalSeconds);
                options.Retry.MaxRetryAttempts = Math.Max(0, httpOptions.RetryCount);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(samplingSeconds);
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = Math.Max(2, httpOptions.CircuitBreakerFailures);
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(Math.Max(1, httpOptions.CircuitBreakerDurationSeconds));
            });

        services.AddSingleton<IResilientHttpClient, ResilientHttpClient>();
        services.AddHostedService<SchedulerHostedService>();

        return services;
    }
}
