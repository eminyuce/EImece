using EImece.Domain.Configuration;
using EImece.Domain.Observability.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace EImece.Domain.Configuration
{
    public static class EimeceHttpClientRegistration
    {
        public static IServiceCollection AddEimeceHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient(HttpClientNames.Resilient, ConfigureResilientClient);
            services.AddHttpClient(HttpClientNames.Iyzico, ConfigureIyzicoClient);
            services.AddHttpClient(HttpClientNames.Recaptcha, ConfigureRecaptchaClient);
            services.AddHttpClient(HttpClientNames.ExternalApi, ConfigureExternalApiClient);
            return services;
        }

        private static void ConfigureResilientClient(IServiceProvider sp, HttpClient client)
        {
            var options = sp.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds + 5);
        }

        private static void ConfigureIyzicoClient(IServiceProvider sp, HttpClient client)
        {
            var options = sp.GetRequiredService<IOptions<IyzicoOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                var baseUrl = options.BaseUrl.Trim();
                if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
                {
                    baseUrl += "/";
                }

                client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            }

            client.Timeout = TimeSpan.FromSeconds(30);
        }

        private static void ConfigureRecaptchaClient(IServiceProvider sp, HttpClient client)
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        }

        private static void ConfigureExternalApiClient(IServiceProvider sp, HttpClient client)
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        }
    }
}
