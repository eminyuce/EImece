using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class EntityExtensionQaFixTests
    {
        [TestMethod]
        public void GetDetailPageUrl_WithoutHttpContext_UsesCanonicalCategoryRoutes()
        {
            var category = new ProductCategory { Id = 12, Name = "Elektronik" };
            var url = category.GetDetailPageUrl("Category", "ProductCategories", "", "http");
            StringAssert.Contains(url, "/c/pc/");
            StringAssert.DoesNotMatch(url, new System.Text.RegularExpressions.Regex("/productcategories/category/", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        }

        [TestMethod]
        public void GetDetailPageUrl_WithoutHttpContext_UsesCanonicalStoryCategoryRoutes()
        {
            var category = new StoryCategory { Id = 4, Name = "Stil Rehberi" };
            var url = category.GetDetailPageUrl("Categories", "Stories", "", "http");
            StringAssert.Contains(url, "/s/sc/");
        }

        [TestMethod]
        public void BuildMenuLinkRelativePath_UsesCanonicalStorefrontRoutes()
        {
            var menu = new Menu { Id = 9, Name = "Blog" };

            Assert.AreEqual("/s/", EntityExtension.BuildMenuLinkRelativePath("stories", "index", null));
            Assert.AreEqual("/p/", EntityExtension.BuildMenuLinkRelativePath("products", "index", null));
            Assert.AreEqual("/", EntityExtension.BuildMenuLinkRelativePath("home", "index", null));
            Assert.AreEqual("/s/sc/my-category-abc123", EntityExtension.BuildMenuLinkRelativePath("stories", "categories", "my-category-abc123"));
            Assert.AreEqual("/c/pc/cat-xyz789", EntityExtension.BuildMenuLinkRelativePath("productcategories", "category", "cat-xyz789"));
            Assert.AreEqual("/info/aboutus", EntityExtension.BuildMenuLinkRelativePath("info", "aboutus", null));
            StringAssert.Contains(
                EntityExtension.BuildMenuLinkRelativePath("pages", "detail", null, menu),
                "/i/blog-");
            Assert.AreEqual(string.Empty, EntityExtension.BuildMenuLinkRelativePath("pages", "index", null));
        }

        [TestMethod]
        public void NormalizeImageDimensions_FillsZeroSide()
        {
            int w = 0;
            int h = 500;
            EntityExtension.NormalizeImageDimensions(ref w, ref h);
            Assert.AreEqual(500, w);
            Assert.AreEqual(500, h);

            w = 610;
            h = 0;
            EntityExtension.NormalizeImageDimensions(ref w, ref h);
            Assert.AreEqual(610, w);
            Assert.AreEqual(610, h);

            w = 0;
            h = 0;
            EntityExtension.NormalizeImageDimensions(ref w, ref h);
            Assert.AreEqual(0, w);
            Assert.AreEqual(0, h);
        }
    }
}
