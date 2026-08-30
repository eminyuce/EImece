using EImece.Domain;
using EImece.Domain.Abstractions;
using System;
using System.Web;

namespace EImece.Infrastructure
{
    public class WebSiteUrlProvider : ISiteUrlProvider
    {
        public string GetSiteBaseUrl()
        {
            var request = HttpContext.Current?.Request;
            if (request != null && request.Url != null)
            {
                var appPath = request.ApplicationPath?.TrimEnd('/') ?? string.Empty;
                return $"{request.Url.Scheme}://{request.Url.Authority}{appPath}";
            }

            var protocol = AppConfig.HttpProtocol;
            var domain = AppConfig.Domain;
            if (!string.IsNullOrEmpty(domain))
            {
                return $"{protocol}://{domain.TrimEnd('/')}";
            }

            return $"{protocol}://localhost";
        }

        public string GetSiteDomainUrl()
        {
            var request = HttpContext.Current?.Request;
            if (request != null && request.Url != null)
            {
                var builder = new UriBuilder(AppConfig.HttpProtocol, request.Url.Host, request.Url.Port);
                return builder.Uri.ToString().TrimEnd('/');
            }

            var domain = AppConfig.Domain;
            if (!string.IsNullOrEmpty(domain))
            {
                return $"{AppConfig.HttpProtocol}://{domain.TrimEnd('/')}";
            }

            return $"{AppConfig.HttpProtocol}://localhost";
        }
    }
}
