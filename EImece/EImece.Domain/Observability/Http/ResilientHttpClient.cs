using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Observability.Metrics;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.Http
{
    public sealed class ResilientHttpClient : IResilientHttpClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncPolicyWrap<HttpResponseMessage> _policy;
        private readonly ILogger<ResilientHttpClient> _logger;
        private readonly IApplicationMetrics _metrics;
        private readonly ObservabilityOptions _options;

        public ResilientHttpClient(ILogger<ResilientHttpClient> logger, IApplicationMetrics metrics, ObservabilityOptions options)
        {
            _logger = logger;
            _metrics = metrics;
            _options = options ?? ObservabilityOptions.FromAppConfig();

            _httpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            _policy = Policy.WrapAsync(
                BuildRetryPolicy(),
                BuildCircuitBreakerPolicy(),
                BuildTimeoutPolicy());
        }

        public Task<HttpResponsePayload> GetAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetAsync(url, null, cancellationToken);
        }

        public async Task<HttpResponsePayload> GetAsync(string url, Dictionary<string, string> responseHeaders, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await SendAsync(HttpMethod.Get, url, null, cancellationToken).ConfigureAwait(false))
            {
                var payload = new HttpResponsePayload
                {
                    StatusCode = (int)response.StatusCode,
                    Content = response.Content == null ? null : await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false),
                    ContentType = response.Content?.Headers?.ContentType?.MediaType,
                    Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                };

                if (responseHeaders != null)
                {
                    foreach (var header in response.Headers)
                    {
                        responseHeaders[header.Key] = string.Join(",", header.Value);
                    }

                    if (payload.ContentType != null)
                    {
                        responseHeaders["ContentType"] = payload.ContentType;
                    }
                }

                return payload;
            }
        }

        public async Task<byte[]> GetByteRangeAsync(string url, int startRange, int endRange, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Range = new RangeHeaderValue(startRange, endRange);
                using (var response = await SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    return response.Content == null ? null : await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await SendAsync(HttpMethod.Get, url, null, cancellationToken).ConfigureAwait(false))
            {
                if (response.Content == null)
                {
                    return null;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return bytes == null ? null : System.Text.Encoding.UTF8.GetString(bytes);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, HttpContent content, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(method, url) { Content = content };
            return SendAsync(request, cancellationToken);
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var retryCount = 0;

            var context = new Context
            {
                ["retry_count"] = 0
            };

            try
            {
                var response = await _policy.ExecuteAsync(
                    async (ctx, token) =>
                    {
                        retryCount = ctx.ContainsKey("retry_count") ? (int)ctx["retry_count"] : 0;
                        var clone = await CloneRequestAsync(request).ConfigureAwait(false);
                        return await _httpClient.SendAsync(clone, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
                    },
                    context,
                    cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();
                _metrics.RecordHttpCall(request.RequestUri.ToString(), request.Method.Method, (int)response.StatusCode, stopwatch.ElapsedMilliseconds, retryCount);
                _logger.LogInformation(
                    "HTTP {HttpMethod} {Url} responded {StatusCode} in {DurationMs}ms with {RetryCount} retries. CorrelationId={CorrelationId}",
                    request.Method.Method,
                    SensitiveDataMasker.Mask(request.RequestUri.ToString()),
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    retryCount,
                    CorrelationIdContext.Current);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _metrics.RecordHttpCall(request.RequestUri.ToString(), request.Method.Method, 0, stopwatch.ElapsedMilliseconds, retryCount);
                _logger.LogError(
                    ex,
                    "HTTP {HttpMethod} {Url} failed after {DurationMs}ms with {RetryCount} retries. CorrelationId={CorrelationId}",
                    request.Method.Method,
                    SensitiveDataMasker.Mask(request.RequestUri.ToString()),
                    stopwatch.ElapsedMilliseconds,
                    retryCount,
                    CorrelationIdContext.Current);
                throw;
            }
        }

        private AsyncRetryPolicy<HttpResponseMessage> BuildRetryPolicy()
        {
            var jitter = new Random();

            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .Or<TimeoutRejectedException>()
                .OrResult(response => ShouldRetry(response))
                .WaitAndRetryAsync(
                    _options.HttpRetryCount,
                    retryAttempt =>
                    {
                        var exponential = Math.Pow(2, retryAttempt);
                        var jitterMs = jitter.Next(0, 250);
                        return TimeSpan.FromSeconds(exponential) + TimeSpan.FromMilliseconds(jitterMs);
                    },
                    (outcome, delay, retryAttempt, context) =>
                    {
                        context["retry_count"] = retryAttempt;
                        _logger.LogWarning(
                            "Retrying HTTP call after {DelayMs}ms. Attempt={RetryAttempt} StatusCode={StatusCode} CorrelationId={CorrelationId}",
                            delay.TotalMilliseconds,
                            retryAttempt,
                            outcome.Result != null ? (int?)outcome.Result.StatusCode : null,
                            CorrelationIdContext.Current);
                    });
        }

        private AsyncCircuitBreakerPolicy<HttpResponseMessage> BuildCircuitBreakerPolicy()
        {
            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .Or<TimeoutRejectedException>()
                .OrResult(response => ShouldRetry(response))
                .CircuitBreakerAsync(
                    _options.HttpCircuitBreakerFailures,
                    TimeSpan.FromSeconds(_options.HttpCircuitBreakerDurationSeconds),
                    (result, duration) =>
                    {
                        _logger.LogWarning(
                            "HTTP circuit opened for {DurationSeconds}s. CorrelationId={CorrelationId}",
                            duration.TotalSeconds,
                            CorrelationIdContext.Current);
                    },
                    () =>
                    {
                        _logger.LogInformation(
                            "HTTP circuit reset. CorrelationId={CorrelationId}",
                            CorrelationIdContext.Current);
                    });
        }

        private AsyncTimeoutPolicy<HttpResponseMessage> BuildTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(_options.HttpTimeoutSeconds));
        }

        private static bool ShouldRetry(HttpResponseMessage response)
        {
            if (response == null)
            {
                return true;
            }

            var statusCode = (int)response.StatusCode;
            return statusCode == 408
                || statusCode == 429
                || statusCode >= 500;
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            if (request.Content != null)
            {
                var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                clone.Content = new ByteArrayContent(bytes);
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
