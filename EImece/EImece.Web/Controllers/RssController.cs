using System.Text;
using System.Xml.Linq;
using EImece.Domain.Core.Services;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class RssController : BaseController
{
    private readonly IStorefrontService _storefront;

    public RssController(IOptions<EImeceOptions> siteOptions, IStorefrontService storefront)
        : base(siteOptions)
    {
        _storefront = storefront;
    }

    [HttpGet]
    public async Task<IActionResult> Products(int take = 20, int? categoryId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _storefront.GetProductsForRssAsync(take, SiteOptions.MainLanguage, categoryId, cancellationToken).ConfigureAwait(false);
            return Content(BuildProductFeed(items), "application/rss+xml", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            return Content(ex.Message, "text/plain", Encoding.UTF8);
        }
    }

    [HttpGet]
    public async Task<IActionResult> StoryCategories(int take = 20, int? categoryId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _storefront.GetStoriesForRssAsync(take, SiteOptions.MainLanguage, categoryId, cancellationToken).ConfigureAwait(false);
            return Content(BuildStoryFeed(items), "application/rss+xml", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            return Content(ex.Message, "text/plain", Encoding.UTF8);
        }
    }

    private static string BuildProductFeed(IReadOnlyList<EImece.Domain.Core.Entities.Product> products)
    {
        XNamespace atom = "http://www.w3.org/2005/Atom";
        var channel = new XElement("channel",
            new XElement("title", "EImece Ürünler"),
            new XElement("description", "Ürün RSS beslemesi"),
            new XElement("link", "/"),
            products.Select(p =>
            {
                var slug = StorefrontMapping.Slug(p.ProductCategory?.Name);
                return new XElement("item",
                    new XElement("title", p.Name),
                    new XElement("description", p.ShortDescription ?? p.Description ?? p.Name),
                    new XElement("link", $"/p/{slug}/{p.Id}/"),
                    new XElement("guid", p.Id.ToString()),
                    new XElement("pubDate", p.UpdatedDate.ToString("R")));
            }));

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("rss", new XAttribute("version", "2.0"), new XAttribute(XNamespace.Xmlns + "atom", atom), channel));

        return doc.ToString();
    }

    private static string BuildStoryFeed(IReadOnlyList<EImece.Domain.Core.Entities.Story> stories)
    {
        var channel = new XElement("channel",
            new XElement("title", "EImece Hikayeler"),
            new XElement("description", "Hikaye RSS beslemesi"),
            new XElement("link", "/"),
            stories.Select(s =>
            {
                var slug = StorefrontMapping.Slug(s.StoryCategory?.Name);
                return new XElement("item",
                    new XElement("title", s.Name),
                    new XElement("description", s.ShortDescription ?? s.Description ?? s.Name),
                    new XElement("link", $"/s/{slug}/{s.Id}/"),
                    new XElement("guid", s.Id.ToString()),
                    new XElement("pubDate", s.UpdatedDate.ToString("R")));
            }));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("rss", new XAttribute("version", "2.0"), channel));
        return doc.ToString();
    }
}
