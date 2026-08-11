using EImece.Domain.Entities;
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
        private static ProductCategoryViewModel CreateViewModel(SortingType sorting, IEnumerable<Product> products)
        {
            var productList = products.ToList();
            var category = new ProductCategory
            {
                Id = 1,
                Name = "Test",
                DiscountPercantage = null,
                Products = productList
            };
            foreach (var product in productList)
            {
                product.ProductCategory = category;
            }

            return new ProductCategoryViewModel
            {
                ProductCategory = category,
                AllProducts = productList,
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
        public void Products_Newest_OrdersByUpdatedDateDescending()
        {
            var older = new DateTime(2024, 1, 1);
            var newer = new DateTime(2024, 6, 1);
            var products = new List<Product>
            {
                new Product { Id = 1, Price = 10, UpdatedDate = older, Position = 1 },
                new Product { Id = 2, Price = 20, UpdatedDate = newer, Position = 2 },
            };

            var orderedIds = CreateViewModel(SortingType.Newest, products).Products.Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 2, 1 }, orderedIds);
        }

        [TestMethod]
        public void Products_LowHighPrice_OrdersByDiscountedPriceAscending()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Price = 30, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 1 },
                new Product { Id = 2, Price = 10, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 2 },
                new Product { Id = 3, Price = 20, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 3 },
            };

            var orderedIds = CreateViewModel(SortingType.LowHighPrice, products).Products.Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 2, 3, 1 }, orderedIds);
        }

        [TestMethod]
        public void Products_HighLowPrice_OrdersByDiscountedPriceDescending()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Price = 30, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 1 },
                new Product { Id = 2, Price = 10, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 2 },
                new Product { Id = 3, Price = 20, Discount = 0, UpdatedDate = DateTime.UtcNow, Position = 3 },
            };

            var orderedIds = CreateViewModel(SortingType.HighLowPrice, products).Products.Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 1, 3, 2 }, orderedIds);
        }

        [TestMethod]
        public void Products_Popularity_OrdersBySoldCountDescending()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Price = 10, SoldCount = 2, UpdatedDate = DateTime.UtcNow, Position = 1 },
                new Product { Id = 2, Price = 10, SoldCount = 9, UpdatedDate = DateTime.UtcNow, Position = 2 },
                new Product { Id = 3, Price = 10, SoldCount = 5, UpdatedDate = DateTime.UtcNow, Position = 3 },
            };

            var orderedIds = CreateViewModel(SortingType.Popularity, products).Products.Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 2, 3, 1 }, orderedIds);
        }

        [TestMethod]
        public void Products_AverageRating_OrdersByRatingDescending_WhenReviewsEnabled()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Price = 10, Rating = 3.2, UpdatedDate = DateTime.UtcNow, Position = 1 },
                new Product { Id = 2, Price = 10, Rating = 4.8, UpdatedDate = DateTime.UtcNow, Position = 2 },
                new Product { Id = 3, Price = 10, Rating = 4.1, UpdatedDate = DateTime.UtcNow, Position = 3 },
            };

            var orderedIds = CreateViewModel(SortingType.AverageRating, products).Products.Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 2, 3, 1 }, orderedIds);
        }
    }
}
