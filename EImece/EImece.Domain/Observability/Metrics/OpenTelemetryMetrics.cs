using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EImece.Domain.Observability.Metrics
{
    /// <summary>
    /// OpenTelemetry instruments bridged from <see cref="IApplicationMetrics"/>.
    /// Labels are low-cardinality (method, status class, normalized route/operation).
    /// </summary>
    public static class OpenTelemetryMetrics
    {
        private static Counter<long> _httpServerRequests;
        private static Histogram<double> _httpServerDuration;
        private static Counter<long> _httpClientRequests;
        private static Histogram<double> _httpClientDuration;
        private static Counter<long> _dbOperations;
        private static Histogram<double> _dbDuration;
        private static Counter<long> _methodInvocations;
        private static Histogram<double> _methodDuration;
        private static ObservableGauge<int> _healthGauge;
        private static int _healthStatusValue = 1; // 1 = up, 0 = down
        private static bool _initialized;

        public static void Initialize(Meter meter)
        {
            if (meter == null || _initialized)
            {
                return;
            }

            _httpServerRequests = meter.CreateCounter<long>(
                "http.server.requests",
                unit: "{request}",
                description: "HTTP server request count");

            _httpServerDuration = meter.CreateHistogram<double>(
                "http.server.duration",
                unit: "ms",
                description: "HTTP server request duration");

            _httpClientRequests = meter.CreateCounter<long>(
                "http.client.requests",
                unit: "{request}",
                description: "Outbound HTTP request count");

            _httpClientDuration = meter.CreateHistogram<double>(
                "http.client.duration",
                unit: "ms",
                description: "Outbound HTTP request duration");

            _dbOperations = meter.CreateCounter<long>(
                "db.client.operations",
                unit: "{operation}",
                description: "Database operation count");

            _dbDuration = meter.CreateHistogram<double>(
                "db.client.duration",
                unit: "ms",
                description: "Database operation duration");

            _methodInvocations = meter.CreateCounter<long>(
                "eimece.method.invocations",
                unit: "{invocation}",
                description: "Controller/Service method invocation count");

            _methodDuration = meter.CreateHistogram<double>(
                "eimece.method.duration",
                unit: "ms",
                description: "Controller/Service method duration (use backend histogram_quantile for P90/P95/P99)");

            _healthGauge = meter.CreateObservableGauge(
                "eimece.health.status",
                () => _healthStatusValue,
                unit: "{status}",
                description: "Application health (1=UP, 0=DOWN)");

            _initialized = true;
        }

        public static void SetHealthStatus(bool isUp)
        {
            _healthStatusValue = isUp ? 1 : 0;
            if (_healthGauge == null)
            {
                return;
            }
        }

        public static void RecordServerRequest(string httpMethod, string route, int statusCode, double durationMs)
        {
            if (!_initialized)
            {
                return;
            }

            var tags = new TagList
            {
                { "http.request.method", NormalizeMethod(httpMethod) },
                { "http.route", NormalizeRoute(route) },
                { "http.response.status_code", statusCode },
                { "http.status_class", StatusClass(statusCode) }
            };

            _httpServerRequests.Add(1, tags);
            _httpServerDuration.Record(durationMs, tags);
        }

        public static void RecordClientRequest(string httpMethod, string operation, int statusCode, double durationMs, int retryCount)
        {
            if (!_initialized)
            {
                return;
            }

            var tags = new TagList
            {
                { "http.request.method", NormalizeMethod(httpMethod) },
                { "http.operation", NormalizeRoute(operation) },
                { "http.response.status_code", statusCode },
                { "http.status_class", StatusClass(statusCode) },
                { "http.retry_count_bucket", RetryBucket(retryCount) }
            };

            _httpClientRequests.Add(1, tags);
            _httpClientDuration.Record(durationMs, tags);
        }

        public static void RecordDatabaseOperation(string operation, double durationMs, bool success)
        {
            if (!_initialized)
            {
                return;
            }

            var tags = new TagList
            {
                { "db.operation", NormalizeRoute(operation) },
                { "db.success", success ? "true" : "false" }
            };

            _dbOperations.Add(1, tags);
            _dbDuration.Record(durationMs, tags);
        }

        /// <summary>
        /// Records Controller/Service method latency. Labels stay low-cardinality
        /// (layer + type + method name only — never argument values).
        /// Backends compute P90/P95/P99 from this histogram's buckets.
        /// </summary>
        public static void RecordMethodDuration(string layer, string typeName, string methodName, double durationMs, bool success)
        {
            if (!_initialized)
            {
                return;
            }

            var tags = new TagList
            {
                { "eimece.layer", string.IsNullOrWhiteSpace(layer) ? "unknown" : layer },
                { "eimece.type", NormalizeTypeName(typeName) },
                { "eimece.method", NormalizeMethodName(methodName) },
                { "eimece.success", success ? "true" : "false" }
            };

            _methodInvocations.Add(1, tags);
            _methodDuration.Record(durationMs, tags);
        }

        public static string NormalizeTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return "Unknown";
            }

            var value = typeName.Trim();
            // Strip generic arity markers: IList`1 → IList
            var tick = value.IndexOf('`');
            if (tick > 0)
            {
                value = value.Substring(0, tick);
            }

            if (value.Length > 80)
            {
                value = value.Substring(0, 80);
            }

            return value;
        }

        public static string NormalizeMethodName(string methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                return "Unknown";
            }

            var value = methodName.Trim();
            if (value.Length > 80)
            {
                value = value.Substring(0, 80);
            }

            return value;
        }

        public static string NormalizeMethod(string method)
        {
            if (string.IsNullOrWhiteSpace(method))
            {
                return "UNKNOWN";
            }

            return method.Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Strips query strings and replaces numeric path segments to control cardinality.
        /// Never use raw full URLs as metric labels.
        /// </summary>
        public static string NormalizeRoute(string routeOrUrl)
        {
            if (string.IsNullOrWhiteSpace(routeOrUrl))
            {
                return "unknown";
            }

            var value = routeOrUrl.Trim();
            var queryIndex = value.IndexOf('?');
            if (queryIndex >= 0)
            {
                value = value.Substring(0, queryIndex);
            }

            // Drop scheme/host if a full URL slipped in.
            if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(value);
                    value = uri.AbsolutePath;
                }
                catch (UriFormatException)
                {
                    // Keep trimmed value.
                }
            }

            var parts = value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parts.Length; i++)
            {
                if (IsHighCardinalitySegment(parts[i]))
                {
                    parts[i] = "{id}";
                }
            }

            var normalized = string.Join(".", parts);
            if (normalized.Length > 80)
            {
                normalized = normalized.Substring(0, 80);
            }

            return string.IsNullOrEmpty(normalized) ? "root" : normalized;
        }

        private static bool IsHighCardinalitySegment(string segment)
        {
            if (string.IsNullOrEmpty(segment))
            {
                return false;
            }

            Guid guid;
            if (Guid.TryParse(segment, out guid))
            {
                return true;
            }

            long number;
            if (long.TryParse(segment, out number))
            {
                return true;
            }

            // Hex tokens / order guids without dashes
            if (segment.Length >= 16)
            {
                var allHex = true;
                for (var i = 0; i < segment.Length; i++)
                {
                    var c = segment[i];
                    if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    {
                        allHex = false;
                        break;
                    }
                }

                if (allHex)
                {
                    return true;
                }
            }

            return false;
        }

        private static string StatusClass(int statusCode)
        {
            if (statusCode <= 0)
            {
                return "error";
            }

            return ((statusCode / 100) * 100).ToString() + "xx";
        }

        private static string RetryBucket(int retryCount)
        {
            if (retryCount <= 0)
            {
                return "0";
            }

            if (retryCount == 1)
            {
                return "1";
            }

            if (retryCount <= 3)
            {
                return "2-3";
            }

            return "4+";
        }
    }
}
