using EImece.Domain.Entities;
using EImece.Domain.Models;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Models.Payment;
using EImece.Domain.Repositories;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class ShoppingCartServiceBusinessLogicTests
    {
        private class FakeDbContextProxy : RealProxy
        {
            public FakeDbContextProxy() : base(typeof(EImece.Domain.DbContext.IEImeceContext)) { }

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                return new ReturnMessage(null, null, 0, call.LogicalCallContext, call);
            }

            public EImece.Domain.DbContext.IEImeceContext Context
            {
                get { return (EImece.Domain.DbContext.IEImeceContext)GetTransparentProxy(); }
            }
        }

        private class FakeAddressService : AddressService
        {
            public List<Address> SavedAddresses { get; } = new List<Address>();

            public FakeAddressService()
                : base(new AddressRepository(new FakeDbContextProxy().Context, TestNullLoggers.Create<AddressRepository>()), TestNullLoggers.Create<AddressService>())
            {
            }

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
            public Customer ExistingCustomer { get; set; }

            public FakeCustomerService()
                : base(new CustomerRepository(new FakeDbContextProxy().Context, TestNullLoggers.Create<CustomerRepository>()), TestNullLoggers.Create<CustomerService>())
            {
            }

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

            public override Customer GetUserId(string userId)
            {
                return ExistingCustomer;
            }

            public override Task<Customer> GetUserIdAsync(string userId)
            {
                return Task.FromResult(ExistingCustomer);
            }
        }

        private class FakeOrderService : OrderService
        {
            public List<Order> SavedOrders { get; } = new List<Order>();

            public FakeOrderService()
                : base(new OrderRepository(new FakeDbContextProxy().Context, TestNullLoggers.Create<OrderRepository>()), TestNullLoggers.Create<OrderService>())
            {
            }

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

            public FakeOrderProductService()
                : base(new OrderProductRepository(new FakeDbContextProxy().Context), TestNullLoggers.Create<OrderProductService>())
            {
            }

            public override OrderProduct SaveOrEditEntity(OrderProduct entity)
            {
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

            public FakeProductService()
                : base(new ProductRepository(new FakeDbContextProxy().Context, TestNullLoggers.Create<ProductRepository>()), null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, TestNullLoggers.Create<ProductService>())
            {
            }

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
            public DateTime? LastCutoffDate { get; private set; }
            public int LastBatchSize { get; private set; }

            public FakeShoppingCartRepository()
                : base(new FakeDbContextProxy().Context, TestNullLoggers.Create<ShoppingCartRepository>())
            {
            }

            public override EImece.Domain.GenericRepository.EntityFramework.EntitiesContext GetDbContext() => null;

            public override System.Data.Entity.DbContextTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel) => null;

            public override System.Data.Entity.DbContextTransaction BeginTransaction() => null;

            public override int DeleteExpiredShoppingCarts(DateTime cutoffDate, int batchSize = 500)
            {
                LastCutoffDate = cutoffDate;
                LastBatchSize = batchSize;
                return 4;
            }

            public override Task<int> DeleteExpiredShoppingCartsAsync(DateTime cutoffDate, int batchSize = 500, CancellationToken cancellationToken = default(CancellationToken))
            {
                LastCutoffDate = cutoffDate;
                LastBatchSize = batchSize;
                return Task.FromResult(4);
            }
        }

        private class FakeCouponValidationService : ICouponValidationService
        {
            public CouponValidationResult Result { get; set; }
            public int CallCount { get; private set; }

            public Task<CouponValidationResult> ValidateCouponAsync(string couponCode, ShoppingCartSession cart, CouponValidationContext context, CancellationToken cancellationToken = default(CancellationToken))
            {
                CallCount++;
                return Task.FromResult(Result);
            }

            public Task<CouponValidationResult> ValidateCouponAsync(string couponCode, BuyWithNoAccountCreation cart, CouponValidationContext context, CancellationToken cancellationToken = default(CancellationToken))
            {
                CallCount++;
                return Task.FromResult(Result);
            }

            public Task<CouponValidationResult> RevalidateActiveCouponAsync(ShoppingCartSession cart, CouponValidationContext context, CancellationToken cancellationToken = default(CancellationToken))
            {
                return ValidateCouponAsync(cart.Coupon.Code, cart, context, cancellationToken);
            }
        }

        private class CouponRedemptionStore
        {
            public List<CouponRedemption> Saved { get; } = new List<CouponRedemption>();

            public int SaveOrEdit(CouponRedemption item)
            {
                Saved.Add(item);
                return Saved.Count;
            }

            public Task<int> SaveOrEditAsync(CouponRedemption item)
            {
                return Task.FromResult(SaveOrEdit(item));
            }
        }

        private static ShoppingCartSession CreateCart(int shippingId = 0, int billingId = 0, CouponDto coupon = null)
        {
            return new ShoppingCartSession
            {
                OrderGuid = Guid.NewGuid().ToString(),
                CurrentLanguage = 1,
                ShippingAddress = new AddressDto { Id = shippingId },
                BillingAddress = new AddressDto { Id = billingId },
                Customer = new CustomerDto { Name = "Ada", Surname = "Lovelace", City = "Istanbul", Country = "TR", ZipCode = "34000", RegistrationAddress = "Street 1" },
                Coupon = coupon,
                CargoPrice = new SettingValueDto { SettingValue = "25" },
                BasketMinTotalPriceForCargo = new SettingValueDto { SettingValue = "500" },
                ShoppingCartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem
                    {
                        Quantity = 2,
                        Product = new ShoppingCartProduct { Id = 10, Name = "Item A", Price = 50, ProductCode = "A-10" }
                    }
                }
            };
        }

        private static PaymentResult CreatePayment()
        {
            return new PaymentResult { PaymentStatus = "SUCCESS", PaidPrice = "100", Currency = "TRY", PaymentId = "pay-1" };
        }

        private ShoppingCartService CreateService(
            FakeAddressService addresses,
            FakeCustomerService customers,
            FakeOrderService orders,
            FakeOrderProductService products,
            FakeProductService stock,
            FakeShoppingCartRepository repository,
            ICouponValidationService coupons = null,
            ICouponRedemptionRepository redemptions = null)
        {
            return new ShoppingCartService(
                null,
                repository,
                orders,
                customers,
                addresses,
                products,
                stock,
                TestNullLoggers.Create<ShoppingCartService>(),
                coupons,
                redemptions);
        }

        [TestMethod]
        public async Task SaveShoppingCartAsync_ThrowsWhenRequiredArgumentsAreMissing()
        {
            var service = CreateService(new FakeAddressService(), new FakeCustomerService(), new FakeOrderService(), new FakeOrderProductService(), new FakeProductService(), new FakeShoppingCartRepository());
            var cart = CreateCart(1, 1);
            var payment = CreatePayment();

            await AssertThrowsAsync<ArgumentNullException>(() => service.SaveShoppingCartAsync("ORD-1", null, payment, "user-1"));
            await AssertThrowsAsync<ArgumentNullException>(() => service.SaveShoppingCartAsync("ORD-1", cart, null, "user-1"));
            await AssertThrowsAsync<ArgumentNullException>(() => service.SaveShoppingCartAsync("ORD-1", cart, payment, null));
            await AssertThrowsAsync<ArgumentNullException>(() => service.SaveShoppingCartAsync("ORD-1", cart, payment, ""));
        }

        [TestMethod]
        public async Task SaveShoppingCartAsync_DoesNotCreateAddressesWhenIdsAlreadyExist()
        {
            var addresses = new FakeAddressService();
            var orders = new FakeOrderService();
            var service = CreateService(addresses, new FakeCustomerService(), orders, new FakeOrderProductService(), new FakeProductService(), new FakeShoppingCartRepository());

            var order = await service.SaveShoppingCartAsync("ORD-EXIST", CreateCart(shippingId: 44, billingId: 55), CreatePayment(), "user-1");

            Assert.AreEqual(0, addresses.SavedAddresses.Count);
            Assert.AreEqual(44, order.ShippingAddressId);
            Assert.AreEqual(55, order.BillingAddressId);
        }

        [TestMethod]
        public async Task SaveShoppingCartAsync_RejectsInvalidCouponBeforePersistingOrder()
        {
            var orders = new FakeOrderService();
            var products = new FakeOrderProductService();
            var stock = new FakeProductService();
            var coupons = new FakeCouponValidationService
            {
                Result = CouponValidationResult.Fail(CouponValidationReason.UsageLimitReached, "Limit reached.", "SAVE10")
            };
            var service = CreateService(new FakeAddressService(), new FakeCustomerService(), orders, products, stock, new FakeShoppingCartRepository(), coupons);
            var cart = CreateCart(1, 1, new CouponDto { Code = "SAVE10" });

            await AssertThrowsAsync<InvalidOperationException>(() => service.SaveShoppingCartAsync("ORD-COUPON", cart, CreatePayment(), "user-1"));

            Assert.AreEqual(1, coupons.CallCount);
            Assert.AreEqual(0, orders.SavedOrders.Count);
            Assert.AreEqual(0, products.SavedOrderProducts.Count);
            Assert.AreEqual(0, stock.DecreasedStocks.Count);
        }

        [TestMethod]
        public async Task SaveShoppingCartAsync_AppliesFreeShippingAndRecordsRedemption()
        {
            var orders = new FakeOrderService();
            var customers = new FakeCustomerService
            {
                ExistingCustomer = new Customer { Id = 17, UserId = "user-1", BirthDate = new DateTime(1990, 5, 1) }
            };
            var redemptionStore = new CouponRedemptionStore();
            var coupons = new FakeCouponValidationService
            {
                Result = CouponValidationResult.Success("FREESHIP", 88, 0, 25, 100)
            };
            var service = CreateService(
                new FakeAddressService(),
                customers,
                orders,
                new FakeOrderProductService(),
                new FakeProductService(),
                new FakeShoppingCartRepository(),
                coupons,
                new FakeServiceProxy<ICouponRedemptionRepository>(redemptionStore).Instance);

            var order = await service.SaveShoppingCartAsync("ORD-SHIP", CreateCart(1, 1, new CouponDto { Code = "FREESHIP" }), CreatePayment(), "user-1");

            Assert.AreEqual(0m, order.CargoPrice);
            Assert.AreEqual("FREESHIP", order.Coupon);
            Assert.AreEqual(1, redemptionStore.Saved.Count);
            Assert.AreEqual(88, redemptionStore.Saved[0].CouponId);
            Assert.AreEqual(order.Id, redemptionStore.Saved[0].OrderId);
            Assert.AreEqual(17, redemptionStore.Saved[0].CustomerId);
            Assert.AreEqual(0m, redemptionStore.Saved[0].DiscountAmount);
            Assert.AreEqual(100m, redemptionStore.Saved[0].OrderTotalBeforeDiscount);
        }

        [TestMethod]
        public async Task SaveBuyWithNoAccountCreationAsync_ThrowsWhenGuestCouponIsInvalid()
        {
            var orders = new FakeOrderService();
            var coupons = new FakeCouponValidationService
            {
                Result = CouponValidationResult.Fail(CouponValidationReason.LoginRequired, "Login required.", "LOGINREQ")
            };
            var service = CreateService(new FakeAddressService(), new FakeCustomerService(), orders, new FakeOrderProductService(), new FakeProductService(), new FakeShoppingCartRepository(), coupons);

            var guest = new BuyWithNoAccountCreation
            {
                OrderGuid = Guid.NewGuid().ToString(),
                Customer = new CustomerDto { UserId = "guest-1", Name = "Guest", Surname = "User" },
                ShippingAddress = new AddressDto { Id = 1 },
                Coupon = new CouponDto { Code = "LOGINREQ" },
                ShoppingCartItems = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem { Quantity = 1, Product = new ShoppingCartProduct { Id = 3, Name = "Mug", Price = 20 } }
                }
            };

            await AssertThrowsAsync<InvalidOperationException>(() => service.SaveBuyWithNoAccountCreationAsync("ORD-GUEST", guest, CreatePayment()));
            Assert.AreEqual(0, orders.SavedOrders.Count);
        }

        [TestMethod]
        public async Task SaveBuyNowAsync_ThrowsWhenSessionOrPaymentIsNull()
        {
            var service = CreateService(new FakeAddressService(), new FakeCustomerService(), new FakeOrderService(), new FakeOrderProductService(), new FakeProductService(), new FakeShoppingCartRepository());

            await AssertThrowsAsync<ArgumentNullException>(() => service.SaveBuyNowAsync(null, CreatePayment()));
            await AssertThrowsAsync<ArgumentNullException>(() => service.SaveBuyNowAsync(new BuyNowModel { OrderGuid = "g", Customer = new CustomerDto(), ShippingAddress = new AddressDto() }, null));
        }

        [TestMethod]
        public void ClearExpiredShoppingCarts_ClampsInvalidRetentionToThirtyDays()
        {
            var repository = new FakeShoppingCartRepository();
            var service = CreateService(new FakeAddressService(), new FakeCustomerService(), new FakeOrderService(), new FakeOrderProductService(), new FakeProductService(), repository);

            var count = service.ClearExpiredShoppingCarts(0);

            Assert.AreEqual(4, count);
            Assert.IsNotNull(repository.LastCutoffDate);
            var expected = DateTime.Now.AddDays(-30);
            Assert.IsTrue(Math.Abs((repository.LastCutoffDate.Value - expected).TotalMinutes) < 2);
        }

        [TestMethod]
        public async Task ClearExpiredShoppingCartsAsync_UsesRequestedRetentionWindow()
        {
            var repository = new FakeShoppingCartRepository();
            var service = CreateService(new FakeAddressService(), new FakeCustomerService(), new FakeOrderService(), new FakeOrderProductService(), new FakeProductService(), repository);

            await service.ClearExpiredShoppingCartsAsync(7);

            var expected = DateTime.Now.AddDays(-7);
            Assert.IsTrue(Math.Abs((repository.LastCutoffDate.Value - expected).TotalMinutes) < 2);
            Assert.AreEqual(500, repository.LastBatchSize);
        }

        private static async Task AssertThrowsAsync<TException>(Func<Task> action) where TException : Exception
        {
            try
            {
                await action();
                Assert.Fail("Expected " + typeof(TException).Name);
            }
            catch (TException)
            {
            }
        }
    }
}
