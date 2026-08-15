using EImece.Domain.Helpers.SiteMap;
using EImece.Domain.Models.FrontModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class StorefrontSeoAndEmptyStateTests
    {
        [TestMethod]
        public void EmptyStateViewModel_InitializesCorrectly_WithProperties()
        {
            var model = new EmptyStateViewModel(
                title: "No Results",
                message: "Try searching something else",
                actionUrl: "/home",
                actionText: "Go Home",
                iconClass: "fa fa-search"
            );

            Assert.AreEqual("No Results", model.Title);
            Assert.AreEqual("Try searching something else", model.Message);
            Assert.AreEqual("/home", model.ActionUrl);
            Assert.AreEqual("Go Home", model.ActionText);
            Assert.AreEqual("fa fa-search", model.IconClass);
        }

        [TestMethod]
        public void SitemapGenerator_FiltersOutNullAndEmptyUrls()
        {
            var generator = new SitemapGenerator();
            var items = new List<ISitemapItem>
            {
                new SitemapItem("https://example.com/p/1", DateTime.UtcNow, SitemapChangeFrequency.Daily, 1.0),
                new SitemapItem("", DateTime.UtcNow, SitemapChangeFrequency.Daily, 1.0),
                new SitemapItem("   ", DateTime.UtcNow, SitemapChangeFrequency.Daily, 1.0),
                null
            };

            XDocument doc = generator.GenerateSiteMap(items);
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urlElements = doc.Descendants(ns + "url").ToList();

            Assert.AreEqual(1, urlElements.Count);
            Assert.AreEqual("https://example.com/p/1", urlElements.First().Element(ns + "loc")?.Value);
        }

        [TestMethod]
        public void SitemapGenerator_DeduplicatesCaseInsensitiveUrls()
        {
            var generator = new SitemapGenerator();
            var items = new List<ISitemapItem>
            {
                new SitemapItem("https://example.com/p/apple", new DateTime(2026, 1, 1), SitemapChangeFrequency.Daily, 1.0),
                new SitemapItem("https://example.com/P/APPLE", new DateTime(2026, 8, 1), SitemapChangeFrequency.Daily, 1.0),
                new SitemapItem("https://example.com/p/banana", new DateTime(2026, 5, 1), SitemapChangeFrequency.Daily, 1.0)
            };

            XDocument doc = generator.GenerateSiteMap(items);
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var locs = doc.Descendants(ns + "loc").Select(e => e.Value).ToList();

            Assert.AreEqual(2, locs.Count);
            CollectionAssert.Contains(locs, "https://example.com/p/apple");
            CollectionAssert.Contains(locs, "https://example.com/p/banana");

            // Verify latest lastmod is preserved
            var appleUrl = doc.Descendants(ns + "url")
                .First(u => u.Element(ns + "loc")?.Value == "https://example.com/p/apple");
            Assert.AreEqual("2026-08-01", appleUrl.Element(ns + "lastmod")?.Value);
        }

        [TestMethod]
        public void SitemapGenerator_HandlesEmptyList_WithoutThrowing()
        {
            var generator = new SitemapGenerator();
            XDocument doc = generator.GenerateSiteMap(new List<ISitemapItem>());

            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urlElements = doc.Descendants(ns + "url").ToList();
            Assert.AreEqual(0, urlElements.Count);
        }
    }
}
