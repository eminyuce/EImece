namespace EImece.Domain.Core.Http;

public sealed class ResilientHttpClient : IResilientHttpClient
{
    public const string HttpClientName = "eimece-resilient";

    private readonly IHttpClientFactory _httpClientFactory;

    public ResilientHttpClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task<string> GetStringAsync(string requestUri, CancellationToken cancellationToken = default)
        => CreateClient().GetStringAsync(requestUri, cancellationToken);

    public Task<byte[]> GetByteArrayAsync(string requestUri, CancellationToken cancellationToken = default)
        => CreateClient().GetByteArrayAsync(requestUri, cancellationToken);

    public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        => CreateClient().GetAsync(requestUri, cancellationToken);

    private HttpClient CreateClient() => _httpClientFactory.CreateClient(HttpClientName);
}
