using System.Text;
using System.Xml.Linq;
using EImece.Domain.Core.Services;
using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class SiteMapController : BaseController
{
    private readonly IStorefrontService _storefront;

    public SiteMapController(IOptions<EImeceOptions> siteOptions, IStorefrontService storefront)
        : base(siteOptions)
    {
        _storefront = storefront;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (SiteOptions.IsSiteUnderConstruction)
        {
            return Content(string.Empty, "application/xml");
        }

        try
        {
            var entries = await _storefront.GetSitemapUrlsAsync(cancellationToken).ConfigureAwait(false);
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urlset = new XElement(ns + "urlset",
                entries.Select(e =>
                {
                    var url = new XElement(ns + "url", new XElement(ns + "loc", e.Loc));
                    if (e.LastMod.HasValue)
                    {
                        url.Add(new XElement(ns + "lastmod", e.LastMod.Value.ToString("yyyy-MM-dd")));
                    }

                    return url;
                }));

            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null), urlset);
            var sb = new StringBuilder();
            using (var writer = System.Xml.XmlWriter.Create(sb, new System.Xml.XmlWriterSettings { OmitXmlDeclaration = false, Indent = true }))
            {
                xml.Save(writer);
            }

            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }
        catch
        {
            var fallback = """
                <?xml version="1.0" encoding="UTF-8"?>
                <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                  <url><loc>/</loc></url>
                </urlset>
                """;
            return Content(fallback, "application/xml", Encoding.UTF8);
        }
    }
}
