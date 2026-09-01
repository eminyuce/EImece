using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
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

                var probeFile = Path.Combine(storageRoot, ".healthcheck_" + Guid.NewGuid().ToString("N"));
                File.WriteAllText(probeFile, DateTime.UtcNow.ToString("o"));
                var canRead = File.ReadAllText(probeFile);
                File.Delete(probeFile);

                var data = new Dictionary<string, object>
                {
                    { "StorageRoot", storageRoot },
                    { "Permissions", "Read / Write (Granted)" },
                    { "DirectoryExists", true }
                };

                string driveInfoText = string.Empty;
                try
                {
                    var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(storageRoot)));
                    if (drive.IsReady)
                    {
                        var freeGb = Math.Round((double)drive.AvailableFreeSpace / (1024 * 1024 * 1024), 2);
                        var totalGb = Math.Round((double)drive.TotalSize / (1024 * 1024 * 1024), 2);
                        data["Drive"] = drive.Name;
                        data["FreeSpaceGB"] = freeGb;
                        data["TotalSpaceGB"] = totalGb;
                        driveInfoText = string.Format(" ({0} GB free on {1})", freeGb, drive.Name);
                    }
                }
                catch
                {
                    // DriveInfo might fail in constrained hosting
                }

                var description = string.Format("File storage read/write verified at '{0}'{1}", storageRoot, driveInfoText);
                return Task.FromResult(HealthCheckResult.Healthy(description, data));
            }
            catch (Exception ex)
            {
                var failData = new Dictionary<string, object>
                {
                    { "StorageRoot", AppConfig.StorageRoot ?? "not configured" },
                    { "Permissions", "Access Denied / Failed" }
                };
                return Task.FromResult(HealthCheckResult.Unhealthy(ex.Message, ex, failData));
            }
        }
    }
}
