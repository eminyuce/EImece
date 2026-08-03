using EImece.Domain.Helpers;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class SqlServerHealthCheck : IHealthCheck
    {
        public string Name
        {
            get { return "sqlServer"; }
        }

        public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            string connectionString;
            try
            {
                connectionString = ConnectionStringProvider.GetConnectionString();
            }
            catch (ConfigurationErrorsException ex)
            {
                return HealthCheckResult.Down(Name, ex.Message);
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

                return HealthCheckResult.Up(Name, "connection alive");
            }
            catch (SqlException ex)
            {
                return HealthCheckResult.Down(Name, ex.Message);
            }
        }
    }
}
