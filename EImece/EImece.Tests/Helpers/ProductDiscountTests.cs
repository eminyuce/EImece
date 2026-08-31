using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ProductDiscountTests
    {
        [TestMethod]
        public void ProductDto_HasDiscount_ReturnsTrue_WhenDirectDiscountPresent()
        {
            var productWithDiscount = new ProductDto
            {
                Price = 100,
                Discount = 20
            };

            Assert.IsTrue(productWithDiscount.HasDiscount);
        }

        [TestMethod]
        public void ProductDto_HasDiscount_ReturnsFalse_WhenNoDiscount_EvenIfIsCampaignTrue()
        {
            var campaignProductNoDiscount = new ProductDto
            {
                Price = 100,
                Discount = null,
                IsCampaign = true
            };

            Assert.IsFalse(campaignProductNoDiscount.HasDiscount);
        }

        [TestMethod]
        public void ProductDto_IsOnSale_ReturnsTrue_WhenPriceGreaterThanZero_AndStateInStockOrPreOrderOrLimited()
        {
            var p1 = new ProductDto { Price = 50, State = "ProductInStock" };
            var p2 = new ProductDto { Price = 50, State = "PreOrder" };
            var p3 = new ProductDto { Price = 50, State = "LimitedStock" };

            Assert.IsTrue(p1.IsOnSale);
            Assert.IsTrue(p2.IsOnSale);
            Assert.IsTrue(p3.IsOnSale);
        }

        [TestMethod]
        public void ProductDto_IsOnSale_ReturnsFalse_WhenOutOfStockOrDiscontinuedOrPriceZero()
        {
            var pOutOfStock = new ProductDto { Price = 50, State = "ProductOutOfStock" };
            var pDiscontinued = new ProductDto { Price = 50, State = "Discontinued" };
            var pNotForSale = new ProductDto { Price = 50, State = "NotForSale" };
            var pZeroPrice = new ProductDto { Price = 0, State = "ProductInStock" };

            Assert.IsFalse(pOutOfStock.IsOnSale);
            Assert.IsFalse(pDiscontinued.IsOnSale);
            Assert.IsFalse(pNotForSale.IsOnSale);
            Assert.IsFalse(pZeroPrice.IsOnSale);
        }

        [TestMethod]
        public void StorefrontProductCardDto_IsOnSale_MatchesProductIsOnSaleLogic()
        {
            var dtoInStock = new StorefrontProductCardDto { Price = 50, State = "ProductInStock" };
            var dtoPreOrder = new StorefrontProductCardDto { Price = 50, State = "PreOrder" };
            var dtoOutOfStock = new StorefrontProductCardDto { Price = 50, State = "ProductOutOfStock" };
            var dtoNotForSale = new StorefrontProductCardDto { Price = 50, State = "NotForSale" };
            var dtoZeroPrice = new StorefrontProductCardDto { Price = 0, State = "ProductInStock" };

            Assert.IsTrue(dtoInStock.IsOnSale);
            Assert.IsTrue(dtoPreOrder.IsOnSale);
            Assert.IsFalse(dtoOutOfStock.IsOnSale);
            Assert.IsFalse(dtoNotForSale.IsOnSale);
            Assert.IsFalse(dtoZeroPrice.IsOnSale);
        }

        [TestMethod]
        public void Product_DiscountPercentage_RoundsToTwoDecimals_AwayFromZero()
        {
            var product = new Product
            {
                Price = 499m,
                Discount = 49m
            };

            Assert.AreEqual(9.82, product.DiscountPercentage);
            Assert.AreEqual("9,82", product.DiscountPercentage.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")));
        }
    }
}
