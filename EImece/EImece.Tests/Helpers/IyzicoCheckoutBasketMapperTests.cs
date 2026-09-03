using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class IyzicoCheckoutBasketMapperTests
    {
        [TestMethod]
        public void ApplyCart_NullRequest_ThrowsArgumentNullException()
        {
            try
            {
                var cart = new ShoppingCartSession();
                IyzicoCheckoutBasketMapper.ApplyCart(null, cart);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void ApplyCart_NullShoppingCart_ThrowsArgumentNullException()
        {
            try
            {
                var request = new CreateCheckoutFormInitializeRequest();
                IyzicoCheckoutBasketMapper.ApplyCart(request, null);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void ApplyCart_EmptyShoppingCart_SetsZeroPriceAndEmptyList()
        {
            var cart = new ShoppingCartSession();
            var request = new CreateCheckoutFormInitializeRequest();

            IyzicoCheckoutBasketMapper.ApplyCart(request, cart);

            Assert.IsNotNull(request.BasketItems);
            Assert.AreEqual(0, request.BasketItems.Count);
            Assert.AreEqual("0", request.Price);
            Assert.AreEqual("0", request.PaidPrice);
        }

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
                request.BasketItems.Sum(i => decimal.Parse(i.Price, CultureInfo.InvariantCulture))
                    .ToString("F2", CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void QuantityLessThanOne_DefaultsToOne()
        {
            var cart = new ShoppingCartSession();
            cart.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = 0,
                Product = new ShoppingCartProduct { Id = 1, Name = "Pen", ProductCode = "PEN", Price = 15.50m, CategoryName = "Kırtasiye" }
            });
            cart.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = -3,
                Product = new ShoppingCartProduct { Id = 2, Name = "Notebook", ProductCode = "NB", Price = 25.00m, CategoryName = "Kırtasiye" }
            });

            var request = new CreateCheckoutFormInitializeRequest();
            IyzicoCheckoutBasketMapper.ApplyCart(request, cart);

            Assert.AreEqual("15.50", request.BasketItems[0].Price);
            Assert.AreEqual("25.00", request.BasketItems[1].Price);
            Assert.AreEqual("40.50", request.Price);
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

        [TestMethod]
        public void PaidPriceIncludesCouponDiscount()
        {
            var cart = new ShoppingCartSession
            {
                Coupon = new EImece.Domain.Models.DTOs.CouponDto { Code = "TEST15" },
                CouponValidatedDiscount = 15m,
                CargoPrice = new SettingValueDto { SettingValue = "10.00" }
            };
            cart.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = 1,
                Product = new ShoppingCartProduct { Id = 1, Name = "Watch", ProductCode = "W1", Price = 100m }
            });

            var request = new CreateCheckoutFormInitializeRequest();
            IyzicoCheckoutBasketMapper.ApplyCart(request, cart);

            // TotalPrice = 100. TotalPriceWithCargoPrice = 100 - 15 + 10 = 95
            Assert.AreEqual("100.00", request.Price);
            Assert.AreEqual("95.00", request.PaidPrice);
        }

        [TestMethod]
        public void BasketItem_FieldMappings_StrictVerification()
        {
            var cart = new ShoppingCartSession();
            cart.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = 1,
                Product = new ShoppingCartProduct
                {
                    Id = 99,
                    ProductCode = "PRD-99",
                    Name = "Sample Product Name",
                    CategoryName = "Electronics",
                    Price = 50.25m
                }
            });

            var request = new CreateCheckoutFormInitializeRequest();
            IyzicoCheckoutBasketMapper.ApplyCart(request, cart);

            Assert.AreEqual(1, request.BasketItems.Count);
            var item = request.BasketItems[0];
            Assert.AreEqual("PRD-99", item.Id);
            Assert.AreEqual("Sample Product Name", item.Name);
            Assert.AreEqual("Electronics", item.Category1);
            Assert.AreEqual(AppConfig.ShoppingCartItemCategory2, item.Category2);
            Assert.AreEqual(BasketItemType.PHYSICAL.ToString(), item.ItemType);
            Assert.AreEqual("50.25", item.Price);
        }

        [TestMethod]
        public void BasketSumRule_AlwaysEqualsRequestPrice_AcrossMultipleItems()
        {
            var cart = new ShoppingCartSession();
            cart.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = 3,
                Product = new ShoppingCartProduct { Id = 1, ProductCode = "P1", Price = 19.99m }
            });
            cart.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = 2,
                Product = new ShoppingCartProduct { Id = 2, ProductCode = "P2", Price = 49.50m }
            });
            cart.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = 4,
                Product = new ShoppingCartProduct { Id = 3, ProductCode = "P3", Price = 100.33m }
            });

            var request = new CreateCheckoutFormInitializeRequest();
            IyzicoCheckoutBasketMapper.ApplyCart(request, cart);

            decimal expectedSum = 59.97m + 99.00m + 401.32m;
            decimal calculatedSum = request.BasketItems
                .Sum(i => decimal.Parse(i.Price, CultureInfo.InvariantCulture));

            Assert.AreEqual(expectedSum.ToString("F2", CultureInfo.InvariantCulture), request.Price);
            Assert.AreEqual(expectedSum, calculatedSum);
            Assert.IsFalse(request.Price.Contains(","), "Price format must use dot separator");
        }
    }
}
