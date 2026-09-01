namespace EImece.Domain.Abstractions
{
    /// <summary>
    /// Pure domain abstraction providing application site URL information
    /// without any dependency on System.Web, HttpContext, or web frameworks.
    /// </summary>
    public interface ISiteUrlProvider
    {
        string GetSiteBaseUrl();
        string GetSiteDomainUrl();
    }
}
