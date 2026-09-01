using EImece.Domain.Helpers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
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

            var sw = Stopwatch.StartNew();
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                var dataSource = builder.DataSource;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                    using (var command = new SqlCommand("SELECT 1", connection))
                    {
                        await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                sw.Stop();

                var data = new Dictionary<string, object>
                {
                    { "DataSource", dataSource },
                    { "Database", databaseName },
                    { "LatencyMs", sw.ElapsedMilliseconds },
                    { "Query", "SELECT 1 (Success)" }
                };

                var description = string.Format("SQL Server connected to database '{0}' on '{1}' ({2} ms)", databaseName, dataSource, sw.ElapsedMilliseconds);
                return HealthCheckResult.Healthy(description, data);
            }
            catch (SqlException ex)
            {
                sw.Stop();
                var failData = new Dictionary<string, object>
                {
                    { "ErrorCode", ex.Number },
                    { "LatencyMs", sw.ElapsedMilliseconds }
                };
                return HealthCheckResult.Unhealthy(ex.Message, ex, failData);
            }
        }
    }
}
