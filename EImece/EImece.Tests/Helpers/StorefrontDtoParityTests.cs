using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class StorefrontDtoParityTests
    {
        [TestMethod]
        public void StorefrontProductCardDto_ParityProperties_WorkCorrectly()
        {
            var dto = new StorefrontProductCardDto
            {
                Id = 123,
                Name = "Default Name",
                NameShort = "Short Name",
                NameLong = "Long Name",
                Price = 100,
                Discount = 20,
                State = "ProductInStock",
                MainImageId = 456
            };

            Assert.AreEqual(ProductState.ProductInStock, dto.StateEnum);
            Assert.AreEqual("Short Name", dto.ProductNameStr);
            Assert.IsTrue(dto.ImageState);
            Assert.IsTrue(dto.IsOnSale);
            Assert.IsTrue(dto.HasDiscount);
            Assert.IsFalse(string.IsNullOrEmpty(dto.BuyNowRelativeUrl));
        }

        [TestMethod]
        public void StorefrontCategoryDto_ParityProperties_WorkCorrectly()
        {
            var entity = new ProductCategory
            {
                Id = 10,
                Name = "Elektronik",
                ParentId = 0,
                DiscountPercantage = 15.5,
                MainImageId = 99,
                IsActive = true
            };

            var dto = StorefrontCategoryDto.FromEntity(entity);

            Assert.AreEqual(10, dto.Id);
            Assert.AreEqual("Elektronik", dto.Name);
            Assert.IsTrue(dto.ImageState);
            Assert.AreEqual(16, dto.DiscountPercentage);
            Assert.AreEqual(16.0, dto.DiscountPercantage);
            Assert.IsNotNull(dto.Childrens);
            Assert.AreSame(dto.Children, dto.Childrens);
        }

        [TestMethod]
        public void StorefrontMenuDto_ParityProperties_WorkCorrectly()
        {
            var entity = new Menu
            {
                Id = 5,
                Name = "Hakkımızda",
                Link = "https://example.com",
                LinkIsActive = true,
                MainImageId = 12,
                MainPage = true,
                MetaKeywords = "about, company",
                Description = "Şirket açıklaması"
            };

            var dto = StorefrontMenuDto.FromEntity(entity);

            Assert.AreEqual("https://example.com", dto.Url);
            Assert.AreEqual("https://example.com", dto.Link);
            Assert.IsTrue(dto.MainPage);
            Assert.IsTrue(dto.ImageState);
            Assert.AreEqual("about, company", dto.MetaKeywords);
            Assert.AreEqual("Şirket açıklaması", dto.ShortDescription);
            Assert.IsNotNull(dto.Childrens);
            Assert.AreSame(dto.Children, dto.Childrens);
        }

        [TestMethod]
        public void StorefrontBrandDto_ParityProperties_WorkCorrectly()
        {
            var entity = new Brand
            {
                Id = 42,
                Name = "Apple",
                MainPage = true,
                MainImageId = 77,
                Description = "Tech Brand",
                IsActive = true
            };

            var dto = StorefrontBrandDto.FromEntity(entity);

            Assert.AreEqual(42, dto.Id);
            Assert.AreEqual("Apple", dto.Name);
            Assert.IsTrue(dto.MainPage);
            Assert.AreEqual(77, dto.MainImageId);
            Assert.IsTrue(dto.ImageState);
            Assert.IsFalse(string.IsNullOrEmpty(dto.DetailPageUrl));
            Assert.AreEqual(dto.DetailPageUrl, dto.DetailPageRelativeUrl);
        }

        [TestMethod]
        public void StorefrontProductSpecificationDto_ParityProperties_WorkCorrectly()
        {
            var entity = new ProductSpecification
            {
                Id = 1,
                ProductId = 101,
                Name = "RAM",
                Value = "16",
                Unit = "GB",
                GroupName = "Bellek",
                Position = 2,
                IsActive = true
            };

            var dto = StorefrontProductSpecificationDto.FromEntity(entity);

            Assert.AreEqual("GB", dto.Unit);
            Assert.AreEqual("Bellek", dto.GroupName);
            Assert.AreEqual(2, dto.Order);
            Assert.AreEqual(2, dto.Position);
        }

        [TestMethod]
        public void StorefrontProductCommentDto_ParityProperties_WorkCorrectly()
        {
            var entity = new ProductComment
            {
                Id = 1,
                ProductId = 101,
                UserId = "user-1",
                Name = "Ahmet",
                Review = "Harika ürün",
                Email = "ahmet@example.com",
                Subject = "Deneyim",
                Rating = 5,
                IsActive = true
            };

            var dto = StorefrontProductCommentDto.FromEntity(entity);

            Assert.AreEqual("Harika ürün", dto.Comment);
            Assert.AreEqual("Harika ürün", dto.Review);
            Assert.AreEqual("ahmet@example.com", dto.Email);
            Assert.AreEqual("Deneyim", dto.Subject);
            Assert.AreEqual("user-1", dto.UserId);
            Assert.AreEqual(5, dto.Rating);
        }

        [TestMethod]
        public void StorefrontTagDto_ParityProperties_WorkCorrectly()
        {
            var entity = new Tag
            {
                Id = 7,
                Name = "Yeni Sezon",
                TagCategoryId = 2,
                Position = 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var dto = StorefrontTagDto.FromEntity(entity);

            Assert.AreEqual(7, dto.Id);
            Assert.AreEqual("Yeni Sezon", dto.Name);
            Assert.IsFalse(string.IsNullOrEmpty(dto.DetailPageRelativeUrlForProducts));
            Assert.AreEqual(dto.DetailPageUrl, dto.DetailPageRelativeUrlForProducts);
        }
    }
}
