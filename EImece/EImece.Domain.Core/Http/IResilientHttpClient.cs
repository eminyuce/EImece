namespace EImece.Domain.Core.Http;

/// <summary>
/// Thin outbound HTTP helper (legacy ResilientHttpClient parity).
/// Backed by IHttpClientFactory + Polly resilience pipeline.
/// </summary>
public interface IResilientHttpClient
{
    Task<string> GetStringAsync(string requestUri, CancellationToken cancellationToken = default);
    Task<byte[]> GetByteArrayAsync(string requestUri, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default);
}
