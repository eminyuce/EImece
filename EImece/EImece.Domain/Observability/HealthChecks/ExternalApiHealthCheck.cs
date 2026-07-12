using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class ExternalApiHealthCheck : IHealthCheck
    {
        private static readonly HttpClient SharedClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        public string Name
        {
            get { return "externalApi"; }
        }

        public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            var baseUrl = AppConfig.IyzicoBaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return HealthCheckResult.Down(Name, "External API base URL is not configured.");
            }

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, baseUrl))
                using (var response = await SharedClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    var statusCode = (int)response.StatusCode;
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.MethodNotAllowed)
                    {
                        return HealthCheckResult.Up(Name, statusCode + " reachable");
                    }

                    return HealthCheckResult.Down(Name, statusCode + " " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Down(Name, ex.Message);
            }
        }
    }
}
