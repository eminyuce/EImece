using EImece.Domain.Configuration;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class IyzicoServiceValidationTests
    {
        [TestMethod]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            try
            {
                var options = Options.Create(new IyzicoOptions());
                new IyzicoService(null, options);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void Constructor_NullOptions_ThrowsArgumentNullException()
        {
            try
            {
                var logger = new NullLogger<IyzicoService>();
                new IyzicoService(logger, null);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateCheckoutFormInitializeAsync_NullShoppingCart_ThrowsArgumentNullException()
        {
            try
            {
                var service = CreateServiceWithUnconfiguredOptions();
                await service.CreateCheckoutFormInitializeAsync(null, "user-1");
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateCheckoutFormInitializeAsync_EmptyCartItems_ThrowsArgumentNullException()
        {
            try
            {
                var service = CreateServiceWithUnconfiguredOptions();
                var cart = new ShoppingCartSession();
                await service.CreateCheckoutFormInitializeAsync(cart, "user-1");
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateCheckoutFormInitializeAsync_NullCustomer_ThrowsArgumentNullException()
        {
            try
            {
                var service = CreateServiceWithUnconfiguredOptions();
                var cart = new ShoppingCartSession();
                cart.ShoppingCartItems.Add(new ShoppingCartItem
                {
                    Product = new ShoppingCartProduct { Id = 1, Price = 10m }
                });
                cart.Customer = null;

                await service.CreateCheckoutFormInitializeAsync(cart, "user-1");
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateCheckoutFormInitializeAsync_UnconfiguredCredentials_ThrowsInvalidOperationException()
        {
            try
            {
                var service = CreateServiceWithUnconfiguredOptions();
                var cart = new ShoppingCartSession
                {
                    Customer = new CustomerDto { Id = 1, Name = "A", Surname = "B" }
                };
                cart.ShoppingCartItems.Add(new ShoppingCartItem
                {
                    Product = new ShoppingCartProduct { Id = 1, Price = 10m }
                });

                await service.CreateCheckoutFormInitializeAsync(cart, "user-1");
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateCheckoutFormInitializeBuyNowAsync_UnconfiguredCredentials_ThrowsInvalidOperationException()
        {
            try
            {
                var service = CreateServiceWithUnconfiguredOptions();
                var buyNow = new BuyNowModel
                {
                    OrderGuid = "guid-1",
                    Customer = new CustomerDto { Id = 1 }
                };

                await service.CreateCheckoutFormInitializeBuyNowAsync(buyNow);
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        private static IyzicoService CreateServiceWithUnconfiguredOptions()
        {
            var logger = new NullLogger<IyzicoService>();
            var options = Options.Create(new IyzicoOptions
            {
                ApiKey = "",
                SecretKey = "",
                BaseUrl = "https://sandbox-api.iyzipay.com"
            });
            return new IyzicoService(logger, options);
        }
    }
}
