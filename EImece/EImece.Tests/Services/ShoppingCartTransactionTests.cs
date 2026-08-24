using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Models.Payment;
using EImece.Domain.Repositories;
using EImece.Domain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class ShoppingCartTransactionTests
    {
        private class FakeDbContextProxy : RealProxy
        {
            public FakeDbContextProxy() : base(typeof(IEImeceContext))
            {
            }

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                object defaultResult = null;
                if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType.IsValueType)
                    {
                        defaultResult = Activator.CreateInstance(mi.ReturnType);
                    }
                }
                return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
            }

            public IEImeceContext Context => (IEImeceContext)GetTransparentProxy();
        }

        private class FakeAddressService : AddressService
        {
            public List<Address> SavedAddresses { get; } = new List<Address>();

            public FakeAddressService() : base(new AddressRepository(new FakeDbContextProxy().Context)) { }

            public override Address SaveOrEditEntity(Address entity)
            {
                if (entity.Id == 0) entity.Id = SavedAddresses.Count + 1;
                SavedAddresses.Add(entity);
                return entity;
            }

            public override Task<Address> SaveOrEditEntityAsync(Address entity)
            {
                return Task.FromResult(SaveOrEditEntity(entity));
            }
        }

        private class FakeCustomerService : CustomerService
        {
            public List<Customer> SavedCustomers { get; } = new List<Customer>();
            public List<string> NormalCustomerUserIds { get; } = new List<string>();

            public FakeCustomerService() : base(new CustomerRepository(new FakeDbContextProxy().Context), null) { }

            public override Customer SaveOrEditEntity(Customer entity)
            {
                if (entity.Id == 0) entity.Id = SavedCustomers.Count + 1;
                SavedCustomers.Add(entity);
                return entity;
            }

            public override Task<Customer> SaveOrEditEntityAsync(Customer entity)
            {
                return Task.FromResult(SaveOrEditEntity(entity));
            }

            public override void SaveCustomerTypeToNormal(string userId)
            {
                NormalCustomerUserIds.Add(userId);
            }

            public override Task SaveCustomerTypeToNormalAsync(string userId)
            {
                SaveCustomerTypeToNormal(userId);
                return Task.CompletedTask;
            }
        }

        private class FakeOrderService : OrderService
        {
            public List<Order> SavedOrders { get; } = new List<Order>();

            public FakeOrderService() : base(new OrderRepository(new FakeDbContextProxy().Context), null, null) { }

            public override Order SaveOrEditEntity(Order entity)
            {
                if (entity.Id == 0) entity.Id = SavedOrders.Count + 1;
                SavedOrders.Add(entity);
                return entity;
            }

            public override Task<Order> SaveOrEditEntityAsync(Order entity)
            {
                return Task.FromResult(SaveOrEditEntity(entity));
            }
        }

        private class FakeOrderProductService : OrderProductService
        {
            public List<OrderProduct> SavedOrderProducts { get; } = new List<OrderProduct>();
            public bool ShouldThrowOnSave { get; set; }

            public FakeOrderProductService() : base(new OrderProductRepository(new FakeDbContextProxy().Context)) { }

            public override OrderProduct SaveOrEditEntity(OrderProduct entity)
            {
                if (ShouldThrowOnSave)
                {
                    throw new InvalidOperationException("Simulated order product persistence failure.");
                }
                if (entity.Id == 0) entity.Id = SavedOrderProducts.Count + 1;
                SavedOrderProducts.Add(entity);
                return entity;
            }

            public override Task<OrderProduct> SaveOrEditEntityAsync(OrderProduct entity)
            {
                return Task.FromResult(SaveOrEditEntity(entity));
            }
        }

        private class FakeProductService : ProductService
        {
            public List<Tuple<int, int>> DecreasedStocks { get; } = new List<Tuple<int, int>>();

            public FakeProductService() : base(new ProductRepository(new FakeDbContextProxy().Context)) { }

            public override void DecreaseStock(int productId, int quantity)
            {
                DecreasedStocks.Add(Tuple.Create(productId, quantity));
            }

            public override Task DecreaseStockAsync(int productId, int quantity, CancellationToken cancellationToken = default(CancellationToken))
            {
                DecreaseStock(productId, quantity);
                return Task.CompletedTask;
            }
        }

        private class FakeShoppingCartRepository : ShoppingCartRepository
        {
            public FakeShoppingCartRepository() : base(new FakeDbContextProxy().Context) { }

            public override EImece.Domain.GenericRepository.EntityFramework.EntitiesContext GetDbContext() => null;
        }

        [TestMethod]
        public async Task SaveShoppingCartAsync_ExecutesAllPipelineStepsAndDecrementsStock()
        {
            // Arrange
            var addressService = new FakeAddressService();
            var customerService = new FakeCustomerService();
            var orderService = new FakeOrderService();
            var orderProductService = new FakeOrderProductService();
            var productService = new FakeProductService();
            var repository = new FakeShoppingCartRepository();

            var shoppingCartService = new ShoppingCartService(
                null,
                null,
                repository,
                orderService,
                customerService,
                addressService,
                orderProductService,
                productService);

            var cartSession = new ShoppingCartSession
            {
                OrderGuid = Guid.NewGuid().ToString(),
                ShippingAddress = new AddressDto { Id = 0 },
                BillingAddress = new AddressDto { Id = 0 },
                ShoppingCartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Quantity = 2,
                        Product = new ShoppingCartProduct { Id = 10, Name = "Item A", Price = 50 }
                    },
                    new ShoppingCartItem
                    {
                        Quantity = 1,
                        Product = new ShoppingCartProduct { Id = 20, Name = "Item B", Price = 75 }
                    }
                }
            };

            var paymentResult = new PaymentResult { PaymentStatus = "SUCCESS", PaidPrice = "175" };

            // Act
            var order = await shoppingCartService.SaveShoppingCartAsync("ORD-12345", cartSession, paymentResult, "user-123");

            // Assert
            Assert.IsNotNull(order);
            Assert.AreEqual(1, orderService.SavedOrders.Count);

            // Step 1: Address saved (shipping + billing)
            Assert.IsTrue(addressService.SavedAddresses.Count >= 2);

            // Step 2: Customer type updated
            Assert.IsTrue(customerService.NormalCustomerUserIds.Contains("user-123"));

            // Step 3: Order line items persisted
            Assert.AreEqual(2, orderProductService.SavedOrderProducts.Count);

            // Step 4: Stock decremented for each cart line item
            Assert.AreEqual(2, productService.DecreasedStocks.Count);
            Assert.AreEqual(10, productService.DecreasedStocks[0].Item1);
            Assert.AreEqual(2, productService.DecreasedStocks[0].Item2);
            Assert.AreEqual(20, productService.DecreasedStocks[1].Item1);
            Assert.AreEqual(1, productService.DecreasedStocks[1].Item2);
        }

        [TestMethod]
        public async Task SaveBuyNowAsync_ExecutesPipelineAndDecrementsStock()
        {
            // Arrange
            var addressService = new FakeAddressService();
            var customerService = new FakeCustomerService();
            var orderService = new FakeOrderService();
            var orderProductService = new FakeOrderProductService();
            var productService = new FakeProductService();
            var repository = new FakeShoppingCartRepository();

            var shoppingCartService = new ShoppingCartService(
                null,
                null,
                repository,
                orderService,
                customerService,
                addressService,
                orderProductService,
                productService);

            var buyNowModel = new BuyNowModel
            {
                OrderGuid = Guid.NewGuid().ToString(),
                Customer = new CustomerDto { Id = 0, RegistrationAddress = "Test St", City = "Istanbul", Country = "TR", ZipCode = "34000" },
                ShippingAddress = new AddressDto { Id = 0 },
                ShoppingCartItem = new ShoppingCartItem
                {
                    Quantity = 1,
                    Product = new ShoppingCartProduct { Id = 88, Name = "Special Product", Price = 250 }
                }
            };

            var paymentResult = new PaymentResult { PaymentStatus = "SUCCESS", PaidPrice = "250" };

            // Act
            var order = await shoppingCartService.SaveBuyNowAsync(buyNowModel, paymentResult);

            // Assert
            Assert.IsNotNull(order);
            Assert.AreEqual(1, orderService.SavedOrders.Count);
            Assert.AreEqual(1, customerService.SavedCustomers.Count);
            Assert.AreEqual(1, addressService.SavedAddresses.Count);
            Assert.AreEqual(1, orderProductService.SavedOrderProducts.Count);

            // Stock decremented for buy now item
            Assert.AreEqual(1, productService.DecreasedStocks.Count);
            Assert.AreEqual(88, productService.DecreasedStocks[0].Item1);
            Assert.AreEqual(1, productService.DecreasedStocks[0].Item2);
        }

        [TestMethod]
        public async Task SaveShoppingCartAsync_WhenLineItemFails_ThrowsException()
        {
            // Arrange
            var addressService = new FakeAddressService();
            var customerService = new FakeCustomerService();
            var orderService = new FakeOrderService();
            var orderProductService = new FakeOrderProductService { ShouldThrowOnSave = true };
            var productService = new FakeProductService();
            var repository = new FakeShoppingCartRepository();

            var shoppingCartService = new ShoppingCartService(
                null,
                null,
                repository,
                orderService,
                customerService,
                addressService,
                orderProductService,
                productService);

            var cartSession = new ShoppingCartSession
            {
                OrderGuid = Guid.NewGuid().ToString(),
                ShippingAddress = new AddressDto { Id = 1 },
                BillingAddress = new AddressDto { Id = 1 },
                ShoppingCartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Quantity = 1,
                        Product = new ShoppingCartProduct { Id = 5, Name = "Item 5", Price = 50 }
                    }
                }
            };

            var paymentResult = new PaymentResult { PaymentStatus = "SUCCESS", PaidPrice = "50" };

            // Act & Assert
            bool threwException = false;
            try
            {
                await shoppingCartService.SaveShoppingCartAsync("ORD-FAIL", cartSession, paymentResult, "user-err");
            }
            catch (InvalidOperationException)
            {
                threwException = true;
            }

            Assert.IsTrue(threwException, "Expected InvalidOperationException to be thrown and propagated on line item persistence failure.");
        }
    }
}
