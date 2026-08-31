using EImece.Domain.Configuration;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<IyzicoOptions> _iyzicoOptions;

        public ExternalApiHealthCheck(IHttpClientFactory httpClientFactory, IOptions<IyzicoOptions> iyzicoOptions)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _iyzicoOptions = iyzicoOptions ?? throw new ArgumentNullException(nameof(iyzicoOptions));
        }

        public string Name
        {
            get { return "externalApi"; }
        }

        public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            var baseUrl = _iyzicoOptions.Value?.BaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return HealthCheckResult.Down(Name, "External API base URL is not configured.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientNames.ExternalApi);
                using (var request = new HttpRequestMessage(HttpMethod.Get, baseUrl))
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
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
