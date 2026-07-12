using System;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class RabbitMqHealthCheck : IHealthCheck
    {
        public string Name
        {
            get { return "rabbitmq"; }
        }

        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            var connectionString = AppConfig.GetConfigString("RabbitMqConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return Task.FromResult(HealthCheckResult.Up(Name, "not configured"));
            }

            try
            {
                var factory = new RabbitMQ.Client.ConnectionFactory
                {
                    Uri = new Uri(connectionString)
                };

                using (var connection = factory.CreateConnection())
                {
                    if (connection.IsOpen)
                    {
                        return Task.FromResult(HealthCheckResult.Up(Name, "connection alive"));
                    }
                }

                return Task.FromResult(HealthCheckResult.Down(Name, "connection closed"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Down(Name, ex.Message));
            }
        }
    }
}
