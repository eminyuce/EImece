using EImece.Domain.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class ExternalApiHealthCheck : IHealthCheck
    {
        public const string DefaultName = "externalApi";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<IyzicoOptions> _iyzicoOptions;

        public ExternalApiHealthCheck(IHttpClientFactory httpClientFactory, IOptions<IyzicoOptions> iyzicoOptions)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _iyzicoOptions = iyzicoOptions ?? throw new ArgumentNullException(nameof(iyzicoOptions));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var baseUrl = _iyzicoOptions.Value?.BaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return HealthCheckResult.Unhealthy("External API base URL is not configured.");
            }

            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var client = _httpClientFactory.CreateClient(HttpClientNames.ExternalApi);
                using (var request = new HttpRequestMessage(HttpMethod.Get, baseUrl))
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    sw.Stop();
                    var statusCode = (int)response.StatusCode;
                    var data = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "BaseUrl", baseUrl },
                        { "StatusCode", statusCode },
                        { "StatusText", response.ReasonPhrase },
                        { "LatencyMs", sw.ElapsedMilliseconds }
                    };

                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.MethodNotAllowed)
                    {
                        return HealthCheckResult.Healthy(string.Format("External API reachable at '{0}' (HTTP {1}, {2} ms)", baseUrl, statusCode, sw.ElapsedMilliseconds), data);
                    }

                    return HealthCheckResult.Unhealthy(string.Format("External API returned HTTP {0} ({1})", statusCode, response.ReasonPhrase), null, data);
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }
        }
    }
}
