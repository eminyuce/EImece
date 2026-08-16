using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class CategoryFilterHelperTests
    {
        [TestMethod]
        public void ParseCategoryFilter_PriceFilterP102_Extracts99To499Range()
        {
            // p102 corresponds to index 2 (99 - 499)
            CategoryFilterHelper.ParseCategoryFilter(
                "p102",
                null,
                out var brandIds,
                out var ratings,
                out var priceRanges);

            Assert.AreEqual(0, brandIds.Count);
            Assert.AreEqual(0, ratings.Count);
            Assert.AreEqual(1, priceRanges.Count);
            Assert.AreEqual(99, priceRanges[0].Min);
            Assert.AreEqual(499, priceRanges[0].Max);
        }

        [TestMethod]
        public void ParseCategoryFilter_PriceFilterP103_Extracts499To999Range()
        {
            // p103 corresponds to index 3 (499 - 999)
            CategoryFilterHelper.ParseCategoryFilter(
                "p103",
                null,
                out var brandIds,
                out var ratings,
                out var priceRanges);

            Assert.AreEqual(0, brandIds.Count);
            Assert.AreEqual(0, ratings.Count);
            Assert.AreEqual(1, priceRanges.Count);
            Assert.AreEqual(499, priceRanges[0].Min);
            Assert.AreEqual(999, priceRanges[0].Max);
        }

        [TestMethod]
        public void ParseCategoryFilter_BrandFilterB102_ExtractsBrandId102()
        {
            CategoryFilterHelper.ParseCategoryFilter(
                "b102",
                null,
                out var brandIds,
                out var ratings,
                out var priceRanges);

            Assert.AreEqual(1, brandIds.Count);
            Assert.AreEqual(102, brandIds[0]);
            Assert.AreEqual(0, ratings.Count);
            Assert.AreEqual(0, priceRanges.Count);
        }

        [TestMethod]
        public void ParseCategoryFilter_RatingFilterR5_ExtractsRating5()
        {
            CategoryFilterHelper.ParseCategoryFilter(
                "r5",
                null,
                out var brandIds,
                out var ratings,
                out var priceRanges);

            Assert.AreEqual(0, brandIds.Count);
            Assert.AreEqual(1, ratings.Count);
            Assert.AreEqual(5, ratings[0]);
            Assert.AreEqual(0, priceRanges.Count);
        }

        [TestMethod]
        public void ParseCategoryFilter_CombinedFilters_ExtractsAll()
        {
            CategoryFilterHelper.ParseCategoryFilter(
                "p102-b15-r4-b20",
                null,
                out var brandIds,
                out var ratings,
                out var priceRanges);

            Assert.AreEqual(2, brandIds.Count);
            CollectionAssert.AreEqual(new List<int> { 15, 20 }, brandIds);
            Assert.AreEqual(1, ratings.Count);
            Assert.AreEqual(4, ratings[0]);
            Assert.AreEqual(1, priceRanges.Count);
            Assert.AreEqual(99, priceRanges[0].Min);
            Assert.AreEqual(499, priceRanges[0].Max);
        }

        [TestMethod]
        public void ProductCategoryViewModel_SelectedFilterTypes_ResolvesMatchingFilter()
        {
            var vm = new ProductCategoryViewModel
            {
                Filter = "p102",
                StorefrontBrands = new List<StorefrontBrandDto>
                {
                    new StorefrontBrandDto { Id = 5, Name = "Nike" }
                }
            };

            var selected = vm.SelectedFilterTypes;
            Assert.IsNotNull(selected);
            Assert.AreEqual(1, selected.Count);
            Assert.AreEqual("p102", selected[0].CategoryFilterId);
            Assert.AreEqual(99, selected[0].minPrice);
            Assert.AreEqual(499, selected[0].maxPrice);
        }

        [TestMethod]
        public void ParseCategoryFilter_MultiplePriceFilters_ExtractsAllRanges()
        {
            CategoryFilterHelper.ParseCategoryFilter(
                "p100-p102",
                null,
                out var brandIds,
                out var ratings,
                out var priceRanges);

            Assert.AreEqual(2, priceRanges.Count);
            Assert.AreEqual(0, priceRanges[0].Min);
            Assert.AreEqual(49, priceRanges[0].Max);
            Assert.AreEqual(99, priceRanges[1].Min);
            Assert.AreEqual(499, priceRanges[1].Max);
        }

        [TestMethod]
        public void ParseCategoryFilter_CustomPriceFilterConfig_ExtractsCustomRanges()
        {
            var customSetting = new Setting
            {
                SettingKey = Constants.ProductPriceFilterSetting,
                SettingValue = "{\"PriceRanges\":[{\"Min\":0,\"Max\":100,\"IsLast\":false},{\"Min\":100,\"Max\":500,\"IsLast\":false},{\"Min\":500,\"Max\":9999999,\"IsLast\":true}]}"
            };

            CategoryFilterHelper.ParseCategoryFilter(
                "p101",
                customSetting,
                out var brandIds,
                out var ratings,
                out var priceRanges);

            Assert.AreEqual(1, priceRanges.Count);
            Assert.AreEqual(100, priceRanges[0].Min);
            Assert.AreEqual(500, priceRanges[0].Max);
        }

        [TestMethod]
        public void ParseCategoryFilter_NullOrEmptyOrInvalid_ReturnsEmptyLists()
        {
            CategoryFilterHelper.ParseCategoryFilter(
                null,
                null,
                out var brandIds1,
                out var ratings1,
                out var priceRanges1);

            Assert.AreEqual(0, brandIds1.Count);
            Assert.AreEqual(0, ratings1.Count);
            Assert.AreEqual(0, priceRanges1.Count);

            CategoryFilterHelper.ParseCategoryFilter(
                "invalid-p999-b-r",
                null,
                out var brandIds2,
                out var ratings2,
                out var priceRanges2);

            Assert.AreEqual(0, brandIds2.Count);
            Assert.AreEqual(0, ratings2.Count);
            Assert.AreEqual(0, priceRanges2.Count);
        }

        [TestMethod]
        public void ProductCategoryViewModel_SelectedFilterTypes_ResolvesMultipleFilters()
        {
            var vm = new ProductCategoryViewModel
            {
                Filter = "p102-b5-r4",
                StorefrontBrands = new List<StorefrontBrandDto>
                {
                    new StorefrontBrandDto { Id = 5, Name = "Nike" }
                }
            };

            var selected = vm.SelectedFilterTypes;
            Assert.IsNotNull(selected);
            Assert.AreEqual(3, selected.Count);
            Assert.IsTrue(selected.Any(f => f.CategoryFilterId == "p102"));
            Assert.IsTrue(selected.Any(f => f.CategoryFilterId == "b5"));
            Assert.IsTrue(selected.Any(f => f.CategoryFilterId == "r4"));
        }
    }
}
