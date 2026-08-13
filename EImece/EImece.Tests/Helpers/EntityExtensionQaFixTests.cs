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
