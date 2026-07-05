using Newtonsoft.Json;
using System;

namespace EImece.Domain.Observability.Exceptions
{
    public sealed class ApiErrorResponse
    {
        public string CorrelationId { get; set; }

        public string Message { get; set; }

        public int StatusCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; set; }

        public static ApiErrorResponse Create(int statusCode, string message, string correlationId, string detail = null)
        {
            return new ApiErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                CorrelationId = correlationId,
                Detail = detail
            };
        }
    }
}
