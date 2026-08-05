using EImece.Domain.Core.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EImece.Domain.Core.Hosting;

/// <summary>
/// Quartz-equivalent hosted service. No-ops unless Quartz:IsEnabled is true
/// (default false — matches legacy Web.config and migration roadmap).
/// Full Quartz.NET job scheduling can replace the tick loop in a later phase.
/// </summary>
public sealed class SchedulerHostedService : BackgroundService
{
    private readonly QuartzOptions _options;
    private readonly ILogger<SchedulerHostedService> _logger;

    public SchedulerHostedService(IOptions<QuartzOptions> options, ILogger<SchedulerHostedService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.IsEnabled)
        {
            _logger.LogInformation("Scheduler hosted service is disabled (Quartz:IsEnabled=false).");
            return;
        }

        _logger.LogInformation(
            "Scheduler hosted service started (cron placeholder={Cron}). HelloJob equivalent ticks once per day.",
            _options.HelloJobCron);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        try
        {
            // Immediate first tick for visibility when explicitly enabled.
            await RunHelloJobAsync(stoppingToken).ConfigureAwait(false);

            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                await RunHelloJobAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // graceful shutdown
        }

        _logger.LogInformation("Scheduler hosted service stopped.");
    }

    private Task RunHelloJobAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HelloJob tick at {UtcNow:o}", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}
