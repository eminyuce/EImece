namespace EImece.Domain.Configuration
{
    /// <summary>
    /// Named <see cref="System.Net.Http.IHttpClientFactory"/> client identifiers.
    /// </summary>
    public static class HttpClientNames
    {
        public const string Resilient = "EImece.Resilient";

        public const string Iyzico = "EImece.Iyzico";

        public const string Recaptcha = "EImece.Recaptcha";

        public const string ExternalApi = "EImece.ExternalApi";
    }
}
