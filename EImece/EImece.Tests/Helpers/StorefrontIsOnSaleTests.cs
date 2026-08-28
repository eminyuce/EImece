using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
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
            Assert.IsFalse(dto.IsBuyableState);

            product.Price = -10;
            dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsFalse(dto.IsOnSale);
            Assert.IsFalse(dto.IsBuyableState);
        }

        [TestMethod]
        public void IsOnSale_WhenInStockWithPrice_ShouldReturnTrue()
        {
            var product = new Product { Price = 100, State = ProductState.ProductInStock.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsTrue(dto.IsOnSale);
            Assert.IsTrue(dto.IsBuyableState);
        }

        [TestMethod]
        public void IsOnSale_WhenPreOrderWithPrice_ShouldReturnTrueByDefault()
        {
            var product = new Product { Price = 150, State = ProductState.PreOrder.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsTrue(dto.IsOnSale);
            Assert.IsTrue(dto.IsBuyableState);
        }

        [TestMethod]
        public void IsOnSale_WhenLimitedStockWithPrice_ShouldReturnTrueByDefault()
        {
            var product = new Product { Price = 200, State = ProductState.LimitedStock.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsTrue(dto.IsOnSale);
            Assert.IsTrue(dto.IsBuyableState);
        }

        [TestMethod]
        public void IsOnSale_WhenOutOfStockWithPrice_ShouldReturnFalse()
        {
            var product = new Product { Price = 100, State = ProductState.ProductOutOfStock.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsFalse(dto.IsOnSale);
            Assert.IsFalse(dto.IsBuyableState);
        }

        [TestMethod]
        public void IsOnSale_WhenDiscontinuedWithPrice_ShouldReturnFalse()
        {
            var product = new Product { Price = 100, State = ProductState.Discontinued.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsFalse(dto.IsOnSale);
            Assert.IsFalse(dto.IsBuyableState);
        }

        [TestMethod]
        public void IsOnSale_WhenNotForSaleWithPrice_ShouldReturnFalse()
        {
            var product = new Product { Price = 100, State = ProductState.NotForSale.ToString() };
            var dto = StorefrontProductCardDto.FromEntity(product);

            Assert.IsFalse(dto.IsOnSale);
            Assert.IsFalse(dto.IsBuyableState);
        }

        [TestMethod]
        public void ProductStateHelper_CustomConfiguredStates_ReflectsAdminConfiguration()
        {
            // Admin configures Backorder and ComingSoon as suitable for sale
            string customConfig = "Backorder,ComingSoon,ProductInStock";

            Assert.IsTrue(ProductStateHelper.IsSuitableForSale(ProductState.Backorder, 100, customConfig));
            Assert.IsTrue(ProductStateHelper.IsSuitableForSale(ProductState.ComingSoon, 100, customConfig));
            Assert.IsTrue(ProductStateHelper.IsSuitableForSale(ProductState.ProductInStock, 100, customConfig));

            // PreOrder was excluded by admin in customConfig
            Assert.IsFalse(ProductStateHelper.IsSuitableForSale(ProductState.PreOrder, 100, customConfig));
            Assert.IsFalse(ProductStateHelper.IsSuitableForSale(ProductState.ProductOutOfStock, 100, customConfig));
            Assert.IsFalse(ProductStateHelper.IsSuitableForSale(ProductState.Discontinued, 100, customConfig));
        }

        [TestMethod]
        public void ProductStateHelper_ParseSuitableForSaleStates_HandlesVariousDelimitersAndValues()
        {
            var set = ProductStateHelper.ParseSuitableForSaleStates("ProductInStock, PreOrder; LimitedStock|Backorder 6");

            Assert.IsTrue(set.Contains(ProductState.ProductInStock));
            Assert.IsTrue(set.Contains(ProductState.PreOrder));
            Assert.IsTrue(set.Contains(ProductState.LimitedStock));
            Assert.IsTrue(set.Contains(ProductState.Backorder));
            Assert.IsTrue(set.Contains(ProductState.ComingSoon)); // int 6 = ComingSoon
        }

        [TestMethod]
        public void DtoAndEntity_Parity_ForBuyableAndOnSale()
        {
            var product = new Product { Id = 1, Price = 99.99m, State = "PreOrder" };
            var dto = StorefrontProductCardDto.FromEntity(product);
            var productDto = new ProductDto { Id = 1, Price = 99.99m, State = "PreOrder" };

            Assert.AreEqual(product.IsBuyableState, dto.IsBuyableState);
            Assert.AreEqual(product.IsOnSale, dto.IsOnSale);
            Assert.AreEqual(product.IsBuyableState, productDto.IsBuyableState);
            Assert.AreEqual(product.IsOnSale, productDto.IsOnSale);
        }
    }
}
