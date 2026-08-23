using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class StorefrontIsOnSaleTests
    {
        [TestMethod]
        public void IsOnSale_WhenPriceIsZeroOrNegative_ShouldReturnFalse()
        {
            var product = new Product { Price = 0, State = ProductState.ProductInStock.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsFalse(dto.IsOnSale);

            product.Price = -10;
            dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsFalse(dto.IsOnSale);
        }

        [TestMethod]
        public void IsOnSale_WhenInStockWithPrice_ShouldReturnTrue()
        {
            var product = new Product { Price = 100, State = ProductState.ProductInStock.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsTrue(dto.IsOnSale);
        }

        [TestMethod]
        public void IsOnSale_WhenPreOrderWithPrice_ShouldReturnTrue()
        {
            var product = new Product { Price = 150, State = ProductState.PreOrder.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsTrue(dto.IsOnSale);
        }

        [TestMethod]
        public void IsOnSale_WhenLimitedStockWithPrice_ShouldReturnTrue()
        {
            var product = new Product { Price = 200, State = ProductState.LimitedStock.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsTrue(dto.IsOnSale);
        }

        [TestMethod]
        public void IsOnSale_WhenComingSoonWithPrice_ShouldReturnTrue()
        {
            var product = new Product { Price = 250, State = ProductState.ComingSoon.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsTrue(dto.IsOnSale);
        }

        [TestMethod]
        public void IsOnSale_WhenOutOfStockWithPrice_ShouldReturnFalse()
        {
            var product = new Product { Price = 100, State = ProductState.ProductOutOfStock.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsFalse(dto.IsOnSale);
        }

        [TestMethod]
        public void IsOnSale_WhenDiscontinuedWithPrice_ShouldReturnFalse()
        {
            var product = new Product { Price = 100, State = ProductState.Discontinued.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsFalse(dto.IsOnSale);
        }

        [TestMethod]
        public void IsOnSale_WhenNotForSaleWithPrice_ShouldReturnFalse()
        {
            var product = new Product { Price = 100, State = ProductState.NotForSale.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsFalse(dto.IsOnSale);
        }
    }
}
