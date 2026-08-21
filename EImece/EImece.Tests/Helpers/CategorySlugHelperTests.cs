using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class CategorySlugHelperTests
    {
        [TestMethod]
        public void NormalizeSlug_CollapsesRepeatedDashes()
        {
            Assert.AreEqual("ev-yasam", CategorySlugHelper.NormalizeSlug("Ev--Yasam"));
            Assert.AreEqual("ev-yasam", CategorySlugHelper.NormalizeSlug("/Ev-Yasam/"));
        }

        [TestMethod]
        public void SlugMatchesCategoryName_MatchesLegacyEvYasamBookmark()
        {
            Assert.IsTrue(CategorySlugHelper.SlugMatchesCategoryName("Ev-Yasam", "Ev & Yaşam"));
            Assert.IsTrue(CategorySlugHelper.SlugMatchesCategoryName("ev--yasam", "Ev & Yaşam"));
            Assert.IsFalse(CategorySlugHelper.SlugMatchesCategoryName("elektronik", "Ev & Yaşam"));
        }

        [TestMethod]
        public void FindMatchingCategory_ReturnsActiveCategoryByLegacySlug()
        {
            var tree = new List<ProductCategoryTreeModel>
            {
                new ProductCategoryTreeModel
                {
                    ProductCategory = new StorefrontCategoryDto { Id = 4, Name = "Ev & Yaşam", IsActive = true },
                    Childrens = new List<ProductCategoryTreeModel>()
                },
                new ProductCategoryTreeModel
                {
                    ProductCategory = new StorefrontCategoryDto { Id = 1, Name = "Elektronik", IsActive = true },
                    Childrens = new List<ProductCategoryTreeModel>()
                }
            };

            var match = CategorySlugHelper.FindMatchingCategory(tree, "Ev-Yasam");
            Assert.IsNotNull(match);
            Assert.AreEqual(4, match.Id);
        }

        [TestMethod]
        public void FindMatchingCategory_ReturnsNullWhenMissing()
        {
            var tree = new List<ProductCategoryTreeModel>
            {
                new ProductCategoryTreeModel
                {
                    ProductCategory = new StorefrontCategoryDto { Id = 1, Name = "Elektronik", IsActive = true },
                    Childrens = new List<ProductCategoryTreeModel>()
                }
            };

            Assert.IsNull(CategorySlugHelper.FindMatchingCategory(tree, "yok-boyle-kategori"));
        }
    }
}
