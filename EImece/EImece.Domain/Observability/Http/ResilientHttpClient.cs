using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Observability.Metrics;
using EImece.Domain.Observability.Telemetry;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        // System.Random is not thread-safe; a single instance shared across concurrent retries can
        // corrupt its state and start returning 0, defeating the anti-thundering-herd jitter.
        // ThreadLocal gives each thread its own generator (4.7.2 has no RandomNumberGenerator.GetInt32).
        private static readonly ThreadLocal<Random> Jitter =
            new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        public ResilientHttpClient(ILogger<ResilientHttpClient> logger, IApplicationMetrics metrics, ObservabilityOptions options)
        {
            _logger = logger;
            _metrics = metrics;
            _options = options ?? ObservabilityOptions.FromAppConfig();

            _httpClient = new HttpClient
            {
                // FIX: never InfiniteTimeSpan. Polly's optimistic TimeoutPolicy (BuildTimeoutPolicy)
                // enforces the intended per-attempt timeout via CancellationToken, but if a socket
                // stall fails to observe cancellation, HttpClient.Timeout is the hard backstop that
                // still aborts the attempt. A small buffer above the Polly timeout guarantees Polly
                // wins under normal conditions while capping the absolute per-attempt wall-clock time.
                // This bounds a blocked call to (HttpTimeoutSeconds+buffer) per attempt instead of forever.
                Timeout = TimeSpan.FromSeconds(_options.HttpTimeoutSeconds + 5)
            };

            // Retry (outer) -> Circuit Breaker (middle) -> Timeout (inner, per-attempt).
            // The inner timeout applies to each individual try so a single hung host cannot
            // consume the whole retry budget on one attempt.
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
            var host = request.RequestUri?.Host ?? "unknown";
            var operation = OpenTelemetryMetrics.NormalizeRoute(request.RequestUri?.AbsolutePath ?? host);

            var context = new Context
            {
                ["retry_count"] = 0
            };

            using (var activity = StartClientActivity(request, operation, host))
            {
                try
                {
                    var response = await _policy.ExecuteAsync(
                        async (ctx, token) =>
                        {
                            retryCount = ctx.ContainsKey("retry_count") ? (int)ctx["retry_count"] : 0;
                            var clone = await CloneRequestAsync(request).ConfigureAwait(false);
                            InjectPropagationHeaders(clone);
                            return await _httpClient.SendAsync(clone, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
                        },
                        context,
                        cancellationToken).ConfigureAwait(false);

                    stopwatch.Stop();
                    var statusCode = (int)response.StatusCode;
                    _metrics.RecordHttpCall(operation, request.Method.Method, statusCode, stopwatch.ElapsedMilliseconds, retryCount);

                    if (activity != null)
                    {
                        activity.SetTag(ActivityTags.HttpStatusCode, statusCode);
                        activity.SetTag(ActivityTags.HttpRetryCount, retryCount);
                        if (statusCode >= 500)
                        {
                            activity.SetStatus(ActivityStatusCode.Error, "HTTP " + statusCode);
                        }
                        else
                        {
                            activity.SetStatus(ActivityStatusCode.Ok);
                        }
                    }

                    _logger.LogInformation(
                        "HTTP {HttpMethod} {Url} responded {StatusCode} in {DurationMs}ms with {RetryCount} retries. CorrelationId={CorrelationId} TraceId={TraceId}",
                        request.Method.Method,
                        SensitiveDataMasker.Mask(host + "/" + operation),
                        statusCode,
                        stopwatch.ElapsedMilliseconds,
                        retryCount,
                        CorrelationIdContext.Current,
                        Activity.Current?.TraceId.ToString());

                    return response;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _metrics.RecordHttpCall(operation, request.Method.Method, 0, stopwatch.ElapsedMilliseconds, retryCount);

                    if (activity != null)
                    {
                        activity.SetTag(ActivityTags.HttpRetryCount, retryCount);
                        activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                        activity.AddException(ex);
                    }

                    _logger.LogError(
                        ex,
                        "HTTP {HttpMethod} {Url} failed after {DurationMs}ms with {RetryCount} retries. CorrelationId={CorrelationId} TraceId={TraceId}",
                        request.Method.Method,
                        SensitiveDataMasker.Mask(host + "/" + operation),
                        stopwatch.ElapsedMilliseconds,
                        retryCount,
                        CorrelationIdContext.Current,
                        Activity.Current?.TraceId.ToString());
                    throw;
                }
            }
        }

        private static Activity StartClientActivity(HttpRequestMessage request, string operation, string host)
        {
            var activity = OpenTelemetryBootstrap.ActivitySource?.StartActivity(
                "HTTP " + request.Method.Method,
                ActivityKind.Client);

            if (activity == null)
            {
                return null;
            }

            activity.SetTag(ActivityTags.HttpMethod, request.Method.Method);
            activity.SetTag(ActivityTags.HttpRoute, operation);
            activity.SetTag(ActivityTags.ServerAddress, host);
            activity.SetTag(ActivityTags.CorrelationId, CorrelationIdContext.Current);
            return activity;
        }

        private static void InjectPropagationHeaders(HttpRequestMessage request)
        {
            if (request == null)
            {
                return;
            }

            var correlationId = CorrelationIdContext.Current;
            if (!string.IsNullOrWhiteSpace(correlationId)
                && !request.Headers.Contains(CorrelationIdContext.HeaderName))
            {
                request.Headers.TryAddWithoutValidation(CorrelationIdContext.HeaderName, correlationId);
            }

            var activity = Activity.Current;
            if (activity == null)
            {
                return;
            }

            // W3C trace context
            if (!request.Headers.Contains(CorrelationIdContext.TraceParentHeaderName))
            {
                request.Headers.TryAddWithoutValidation(
                    CorrelationIdContext.TraceParentHeaderName,
                    activity.Id);
            }

            if (!string.IsNullOrEmpty(activity.TraceStateString)
                && !request.Headers.Contains(CorrelationIdContext.TraceStateHeaderName))
            {
                request.Headers.TryAddWithoutValidation(
                    CorrelationIdContext.TraceStateHeaderName,
                    activity.TraceStateString);
            }
        }

        private AsyncRetryPolicy<HttpResponseMessage> BuildRetryPolicy()
        {
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
                        var jitterMs = Jitter.Value.Next(0, 250);
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
