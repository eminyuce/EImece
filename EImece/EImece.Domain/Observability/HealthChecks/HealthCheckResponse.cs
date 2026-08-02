using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class HealthCheckResponse
    {
        public string Status { get; set; }

        public string Version { get; set; }

        public string Timestamp { get; set; }

        public IReadOnlyDictionary<string, HealthCheckComponentResponse> Components { get; private set; }

        public static HealthCheckResponse Create(IReadOnlyCollection<HealthCheckResult> results)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));
            var components = new Dictionary<string, HealthCheckComponentResponse>(results.Count);

            foreach (var result in results)
            {
                var comp = new HealthCheckComponentResponse
                {
                    Status = ToResponseStatus(result.Status),
                    Details = new Dictionary<string, string>
                    {
                        { "message", result.Message }
                    }
                };

                if (result.Status == HealthStatus.Down)
                {
                    comp.Error = result.Message;
                }

                components[result.Name] = comp;
            }

            var overallStatus = results.All(r => r.Status == HealthStatus.Up)
                ? HealthStatus.Up
                : HealthStatus.Down;

            return new HealthCheckResponse
            {
                Status = ToResponseStatus(overallStatus),
                Version = System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "unknown",
                Timestamp = System.DateTime.UtcNow.ToString("o"),
                Components = new ReadOnlyDictionary<string, HealthCheckComponentResponse>(components)
            };
        }

        private static string ToResponseStatus(HealthStatus status)
        {
            return status == HealthStatus.Up ? "UP" : "DOWN";
        }
    }

    public sealed class HealthCheckComponentResponse
    {
        public string Status { get; set; }

        public IReadOnlyDictionary<string, string> Details { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }
    }
}
