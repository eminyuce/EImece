using EImece.Domain.Caching;
using EImece.Domain.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class CacheKeysTests
    {
        [TestMethod]
        public void MainPageProducts_IncludesPageAndLanguage()
        {
            var key = CacheKeys.MainPageProducts(2, 1);
            Assert.AreEqual("product:list:mainpage:p2:lang1", key);
            Assert.StartsWith(CacheKeys.ProductListPrefix, key);
        }

        [TestMethod]
        public void ActiveProductsAsync_UsesDistinctSuffix()
        {
            var sync = CacheKeys.ActiveProducts(1);
            var asyncKey = CacheKeys.ActiveProductsAsync(1);
            Assert.AreEqual("product:list:active:lang1", sync);
            Assert.AreEqual(sync + ":async", asyncKey);
        }

        [TestMethod]
        public void ProductSearch_NormalizesTermAndAvoidsCollisions()
        {
            var a = CacheKeys.ProductSearch("  Shoes ", 1, 12, 1, SortingType.Newest);
            var b = CacheKeys.ProductSearch("shoes", 1, 12, 1, SortingType.Newest);
            var c = CacheKeys.ProductSearch("shoes", 1, 12, 1, SortingType.LowHighPrice);

            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, c);
            Assert.StartsWith(CacheKeys.ProductSearchPrefix, a);
            Assert.Contains("qshoes", a);
        }

        [TestMethod]
        public void NormalizeSearchTerm_CapsLengthAndStripsNoise()
        {
            var longTerm = new string('a', 100) + "!!!";
            var normalized = CacheKeys.NormalizeSearchTerm(longTerm);
            Assert.AreEqual(64, normalized.Length);
            Assert.DoesNotContain("!", normalized);
        }
    }
}
