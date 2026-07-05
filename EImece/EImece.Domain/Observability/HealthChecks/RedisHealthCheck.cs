using System;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class RedisHealthCheck : IHealthCheck
    {
        public string Name
        {
            get { return "redis"; }
        }

        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            var connectionString = AppConfig.GetConfigString("RedisConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return Task.FromResult(HealthCheckResult.Up(Name, "not configured"));
            }

            try
            {
                using (var connection = StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString))
                {
                    var database = connection.GetDatabase();
                    var ping = database.Ping();
                    if (ping.TotalMilliseconds >= 0)
                    {
                        return Task.FromResult(HealthCheckResult.Up(Name, "ping successful"));
                    }
                }

                return Task.FromResult(HealthCheckResult.Down(Name, "ping failed"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Down(Name, ex.Message));
            }
        }
    }
}
