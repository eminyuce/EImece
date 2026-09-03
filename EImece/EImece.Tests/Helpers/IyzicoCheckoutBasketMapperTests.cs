using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using Iyzipay.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class IyzicoCheckoutBasketMapperTests
    {
        [TestMethod]
        public void QuantityIsIncludedInLinePriceAndRequestPrice()
        {
            var cart = new ShoppingCartSession();
            cart.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = 8,
                Product = new ShoppingCartProduct { Id = 1, Name = "Mug", ProductCode = "MUG", Price = 236.68m, CategoryName = "Mutfak" }
            });
            cart.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = 1,
                Product = new ShoppingCartProduct { Id = 2, Name = "Book", ProductCode = "BK", Price = 100m, CategoryName = "Kitap" }
            });

            var request = new CreateCheckoutFormInitializeRequest();
            IyzicoCheckoutBasketMapper.ApplyCart(request, cart);

            Assert.AreEqual("1893.44", request.BasketItems[0].Price);
            Assert.AreEqual("100.00", request.BasketItems[1].Price);
            Assert.AreEqual("1993.44", request.Price);
            Assert.AreEqual(request.Price, request.PaidPrice);
            Assert.AreEqual(
                request.Price,
                request.BasketItems.Sum(i => decimal.Parse(i.Price, System.Globalization.CultureInfo.InvariantCulture))
                    .ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void PaidPriceIncludesCargoWhenPresent()
        {
            var cart = new ShoppingCartSession
            {
                CargoPrice = new SettingValueDto { SettingValue = "25.50" }
            };
            cart.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = 2,
                Product = new ShoppingCartProduct { Id = 1, Name = "Item", ProductCode = "A", Price = 10m }
            });

            var request = new CreateCheckoutFormInitializeRequest();
            IyzicoCheckoutBasketMapper.ApplyCart(request, cart);

            Assert.AreEqual("20.00", request.Price);
            Assert.AreEqual("45.50", request.PaidPrice);
        }
    }
}
