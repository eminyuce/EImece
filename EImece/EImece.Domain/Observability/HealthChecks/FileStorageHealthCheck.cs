using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class FileStorageHealthCheck : IHealthCheck
    {
        public string Name
        {
            get { return "fileStorage"; }
        }

        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            try
            {
                var storageRoot = AppConfig.StorageRoot;
                if (string.IsNullOrWhiteSpace(storageRoot))
                {
                    return Task.FromResult(HealthCheckResult.Down(Name, "Storage root is not configured."));
                }

                if (!Directory.Exists(storageRoot))
                {
                    Directory.CreateDirectory(storageRoot);
                }

                var probeFile = Path.Combine(storageRoot, ".healthcheck");
                File.WriteAllText(probeFile, DateTime.UtcNow.ToString("O"));
                File.Delete(probeFile);

                return Task.FromResult(HealthCheckResult.Up(Name, "read/write available"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Down(Name, ex.Message));
            }
        }
    }
}
