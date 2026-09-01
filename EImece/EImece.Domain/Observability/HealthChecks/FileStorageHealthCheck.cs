using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class FileStorageHealthCheck : IHealthCheck
    {
        public const string DefaultName = "fileStorage";

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var storageRoot = AppConfig.StorageRoot;
                if (string.IsNullOrWhiteSpace(storageRoot))
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy("Storage root is not configured."));
                }

                if (!Directory.Exists(storageRoot))
                {
                    Directory.CreateDirectory(storageRoot);
                }

                var probeFile = Path.Combine(storageRoot, ".healthcheck");
                File.WriteAllText(probeFile, DateTime.UtcNow.ToString("O"));
                File.Delete(probeFile);

                return Task.FromResult(HealthCheckResult.Healthy("read/write available"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(ex.Message, ex));
            }
        }
    }
}
