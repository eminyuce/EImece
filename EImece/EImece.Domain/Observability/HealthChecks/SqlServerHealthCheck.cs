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
            var connectionString = ConfigurationManager.ConnectionStrings[Constants.DbConnectionKey]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return HealthCheckResult.Down(Name, "Connection string is not configured.");
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
