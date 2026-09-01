using EImece.Domain.Helpers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class SqlServerHealthCheck : IHealthCheck
    {
        public const string DefaultName = "sqlServer";

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string connectionString;
            try
            {
                connectionString = ConnectionStringProvider.GetConnectionString();
            }
            catch (ConfigurationErrorsException ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                    using (var command = new SqlCommand("SELECT 1", connection))
                    {
                        await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                    }
                }

                return HealthCheckResult.Healthy("connection alive");
            }
            catch (SqlException ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }
        }
    }
}
