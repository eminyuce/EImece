using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class HealthCheckResponse
    {
        public string Status { get; set; }

        public Dictionary<string, HealthCheckComponentResponse> Components { get; set; }

        public static HealthCheckResponse Create(IReadOnlyCollection<HealthCheckResult> results)
        {
            var details = new Dictionary<string, string>();
            string error = null;

            foreach (var result in results)
            {
                details[result.Name] = result.Message;
                if (result.Status == HealthStatus.Down && error == null)
                {
                    error = result.Message;
                }
            }

            var overallStatus = results.All(r => r.Status == HealthStatus.Up)
                ? HealthStatus.Up
                : HealthStatus.Down;

            var component = new HealthCheckComponentResponse
            {
                Status = ToResponseStatus(overallStatus),
                Details = details
            };

            if (overallStatus == HealthStatus.Down)
            {
                component.Error = error;
            }

            return new HealthCheckResponse
            {
                Status = ToResponseStatus(overallStatus),
                Components = new Dictionary<string, HealthCheckComponentResponse>
                {
                    { "allHealthChecks", component }
                }
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

        public Dictionary<string, string> Details { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }
    }
}
