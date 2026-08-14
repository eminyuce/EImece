using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ProductCategorySortingTests
    {
        private static ProductCategoryViewModel CreateViewModel(SortingType sorting, IEnumerable<StorefrontProductCardDto> dtos)
        {
            var dtoList = dtos.ToList();
            var category = new ProductCategory
            {
                Id = 1,
                Name = "Test",
                DiscountPercantage = null
            };

            var pagedList = new PaginatedList<StorefrontProductCardDto>(dtoList, 1, 20, dtoList.Count);

            return new ProductCategoryViewModel
            {
                ProductCategory = category,
                PagedProductDtos = pagedList,
                AllProducts = new List<Product>(),
                Brands = new List<Brand>(),
                ChildrenProductCategories = new List<ProductCategory>(),
                CategoryChildrenProducts = new List<Product>(),
                Sorting = sorting,
                Filter = string.Empty,
                IsProductPriceEnable = new Setting { SettingValue = "true" },
                IsProductReviewEnable = new Setting { SettingValue = "true" },
                PriceFilterSetting = new Setting { SettingValue = "0-100;100-500;500-1000" }
            };
        }

        [TestMethod]
        public void PagedProductDtos_Newest_PreservesOrder()
        {
            var older = new DateTime(2024, 1, 1);
            var newer = new DateTime(2024, 6, 1);
            var dtos = new List<StorefrontProductCardDto>
            {
                new StorefrontProductCardDto { Id = 2, Price = 20, UpdatedDate = newer, Position = 2 },
                new StorefrontProductCardDto { Id = 1, Price = 10, UpdatedDate = older, Position = 1 },
            };

            var vm = CreateViewModel(SortingType.Newest, dtos);
            var orderedIds = vm.PagedProductDtos.Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 2, 1 }, orderedIds);
        }

        [TestMethod]
        public void PagedProductDtos_LowHighPrice_PreservesOrder()
        {
            var dtos = new List<StorefrontProductCardDto>
            {
                new StorefrontProductCardDto { Id = 2, Price = 10, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 2 },
                new StorefrontProductCardDto { Id = 3, Price = 20, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 3 },
                new StorefrontProductCardDto { Id = 1, Price = 30, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 1 },
            };

            var vm = CreateViewModel(SortingType.LowHighPrice, dtos);
            var orderedIds = vm.PagedProductDtos.Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 2, 3, 1 }, orderedIds);
        }

        [TestMethod]
        public void PagedProductDtos_HighLowPrice_PreservesOrder()
        {
            var dtos = new List<StorefrontProductCardDto>
            {
                new StorefrontProductCardDto { Id = 1, Price = 30, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 1 },
                new StorefrontProductCardDto { Id = 3, Price = 20, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 3 },
                new StorefrontProductCardDto { Id = 2, Price = 10, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 2 },
            };

            var vm = CreateViewModel(SortingType.HighLowPrice, dtos);
            var orderedIds = vm.PagedProductDtos.Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 1, 3, 2 }, orderedIds);
        }

        [TestMethod]
        public void PagedProductDtos_Popularity_PreservesOrder()
        {
            var dtos = new List<StorefrontProductCardDto>
            {
                new StorefrontProductCardDto { Id = 2, Price = 10, SoldCount = 9, UpdatedDate = DateTime.UtcNow, Position = 2 },
                new StorefrontProductCardDto { Id = 3, Price = 10, SoldCount = 5, UpdatedDate = DateTime.UtcNow, Position = 3 },
                new StorefrontProductCardDto { Id = 1, Price = 10, SoldCount = 2, UpdatedDate = DateTime.UtcNow, Position = 1 },
            };

            var vm = CreateViewModel(SortingType.Popularity, dtos);
            var orderedIds = vm.PagedProductDtos.Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 2, 3, 1 }, orderedIds);
        }

        [TestMethod]
        public void PagedProductDtos_AverageRating_PreservesOrder()
        {
            var dtos = new List<StorefrontProductCardDto>
            {
                new StorefrontProductCardDto { Id = 2, Price = 10, Rating = 4.8, UpdatedDate = DateTime.UtcNow, Position = 2 },
                new StorefrontProductCardDto { Id = 3, Price = 10, Rating = 4.1, UpdatedDate = DateTime.UtcNow, Position = 3 },
                new StorefrontProductCardDto { Id = 1, Price = 10, Rating = 3.2, UpdatedDate = DateTime.UtcNow, Position = 1 },
            };

            var vm = CreateViewModel(SortingType.AverageRating, dtos);
            var orderedIds = vm.PagedProductDtos.Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 2, 3, 1 }, orderedIds);
        }

        [TestMethod]
        public void HasAnySoldProducts_IsFalse_WhenNoProductHasSales()
        {
            var dtos = new List<StorefrontProductCardDto>
            {
                new StorefrontProductCardDto { Id = 1, Price = 10, SoldCount = 0, UpdatedDate = DateTime.UtcNow, Position = 1 },
                new StorefrontProductCardDto { Id = 2, Price = 10, SoldCount = 0, UpdatedDate = DateTime.UtcNow, Position = 2 },
            };

            Assert.IsFalse(CreateViewModel(SortingType.Default, dtos).HasAnySoldProducts);
        }

        [TestMethod]
        public void HasAnySoldProducts_IsTrue_WhenAnyProductHasSales()
        {
            var dtos = new List<StorefrontProductCardDto>
            {
                new StorefrontProductCardDto { Id = 1, Price = 10, SoldCount = 0, UpdatedDate = DateTime.UtcNow, Position = 1 },
                new StorefrontProductCardDto { Id = 2, Price = 10, SoldCount = 3, UpdatedDate = DateTime.UtcNow, Position = 2 },
            };

            Assert.IsTrue(CreateViewModel(SortingType.Default, dtos).HasAnySoldProducts);
        }
    }
}
