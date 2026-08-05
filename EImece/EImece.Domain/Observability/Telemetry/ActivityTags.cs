namespace EImece.Domain.Observability.Telemetry
{
    public static class ActivityTags
    {
        public const string CorrelationId = "correlation.id";
        public const string HttpMethod = "http.request.method";
        public const string HttpRoute = "http.route";
        public const string HttpStatusCode = "http.response.status_code";
        public const string HttpRetryCount = "http.retry_count";
        public const string ServerAddress = "server.address";
        public const string DbOperation = "db.operation.name";
        public const string DbSystem = "db.system";
        public const string PaymentProvider = "payment.provider";
        public const string PaymentOperation = "payment.operation";
    }
}
