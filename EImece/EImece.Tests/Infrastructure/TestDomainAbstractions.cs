using EImece.Domain.Abstractions;

namespace EImece.Tests.Infrastructure
{
    internal sealed class NullCurrentUserContext : ICurrentUserContext
    {
        public string GetCurrentUserId() => string.Empty;
        public bool IsAuthenticated => false;
        public bool IsInRole(string role) => false;
    }

    internal sealed class NullSiteUrlProvider : ISiteUrlProvider
    {
        public string GetSiteBaseUrl() => "http://localhost";
        public string GetSiteDomainUrl() => "http://localhost";
    }
}
