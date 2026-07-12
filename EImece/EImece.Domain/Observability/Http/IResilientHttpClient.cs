using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.Http
{
    public sealed class HttpResponsePayload
    {
        public byte[] Content { get; set; }

        public int StatusCode { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string ContentType { get; set; }
    }

    public interface IResilientHttpClient
    {
        Task<HttpResponsePayload> GetAsync(string url, CancellationToken cancellationToken = default(CancellationToken));

        Task<HttpResponsePayload> GetAsync(string url, Dictionary<string, string> responseHeaders, CancellationToken cancellationToken = default(CancellationToken));

        Task<byte[]> GetByteRangeAsync(string url, int startRange, int endRange, CancellationToken cancellationToken = default(CancellationToken));

        Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default(CancellationToken));
    }
}
