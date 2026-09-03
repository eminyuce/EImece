using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class OrderServiceBusinessLogicTests
    {
        private class OrderStore
        {
            public List<Order> Orders { get; } = new List<Order>();
            public List<int> DeletedOrderIds { get; } = new List<int>();

            public Order GetSingle(int id)
            {
                return Orders.FirstOrDefault(o => o.Id == id);
            }

            public Task<Order> GetSingleAsync(int id)
            {
                return Task.FromResult(GetSingle(id));
            }

            public bool DeleteByWhereCondition(Expression<Func<Order, bool>> predicate)
            {
                var match = Orders.Where(predicate.Compile()).ToList();
                foreach (var order in match)
                {
                    Orders.Remove(order);
                    DeletedOrderIds.Add(order.Id);
                }
                return match.Count > 0;
            }

            public Task<bool> DeleteByWhereConditionAsync(Expression<Func<Order, bool>> predicate)
            {
                return Task.FromResult(DeleteByWhereCondition(predicate));
            }

            public IQueryable<Order> FindBy(Expression<Func<Order, bool>> predicate)
            {
                return new FakeAsyncEnumerable<Order>(Orders.Where(predicate.Compile()));
            }

            public List<Order> GetOrdersUserId(string userId, string search)
            {
                return Orders.Where(o => string.Equals(o.UserId, userId, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            public Task<List<Order>> GetOrdersUserIdAsync(string userId, string search)
            {
                return Task.FromResult(GetOrdersUserId(userId, search));
            }

            public Order GetByOrderNumber(string orderNumber)
            {
                return Orders.FirstOrDefault(o => o.OrderNumber == orderNumber);
            }

            public Task<Order> GetByOrderNumberAsync(string orderNumber)
            {
                return Task.FromResult(GetByOrderNumber(orderNumber));
            }

            public Order GetOrderById(int id)
            {
                return GetSingle(id);
            }

            public Task<Order> GetOrderByIdAsync(int id)
            {
                return Task.FromResult(GetSingle(id));
            }
        }

        private class OrderProductStore
        {
            public List<int> DeletedByOrderIds { get; } = new List<int>();

            public bool DeleteOrderProductsByOrderId(int id)
            {
                DeletedByOrderIds.Add(id);
                return true;
            }

            public Task<bool> DeleteOrderProductsByOrderIdAsync(int id)
            {
                DeletedByOrderIds.Add(id);
                return Task.FromResult(true);
            }
        }

        private class AddressStore
        {
            public List<int> DeletedAddressIds { get; } = new List<int>();

            public bool DeleteById(int id)
            {
                DeletedAddressIds.Add(id);
                return true;
            }

            public Task<bool> DeleteByIdAsync(int id)
            {
                DeletedAddressIds.Add(id);
                return Task.FromResult(true);
            }
        }

        private class CustomerStore
        {
            public List<Customer> Customers { get; } = new List<Customer>();
            public List<string> LookedUpUserIds { get; } = new List<string>();

            public Customer GetUserId(string userId)
            {
                LookedUpUserIds.Add(userId);
                return Customers.FirstOrDefault(c => c.UserId == userId);
            }

            public Task<Customer> GetUserIdAsync(string userId)
            {
                return Task.FromResult(GetUserId(userId));
            }
        }

        private static OrderService CreateService(
            OrderStore orders,
            OrderProductStore products = null,
            AddressStore addresses = null,
            CustomerStore customers = null)
        {
            var orderRepo = new FakeServiceProxy<IOrderRepository>(orders).Instance;
            var productService = new FakeServiceProxy<IOrderProductService>(products ?? new OrderProductStore()).Instance;
            var addressService = addresses == null ? null : new FakeServiceProxy<IAddressService>(addresses).Instance;
            var customerService = customers == null ? null : new FakeServiceProxy<ICustomerService>(customers).Instance;
            return new OrderService(orderRepo, TestNullLoggers.Create<OrderService>(), customerService, productService, addressService);
        }

        [TestMethod]
        public void Constructor_ThrowsWhenRepositoryIsNull()
        {
            try
            {
                var unused = new OrderService(null, TestNullLoggers.Create<OrderService>());
                Assert.Fail("Expected ArgumentNullException.");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [TestMethod]
        public void GetByPaymentId_ReturnsNullForBlankPaymentId()
        {
            var service = CreateService(new OrderStore());

            Assert.IsNull(service.GetByPaymentId(null));
            Assert.IsNull(service.GetByPaymentId(""));
            Assert.IsNull(service.GetByPaymentId("   "));
        }

        [TestMethod]
        public async Task GetByPaymentIdAsync_ReturnsNullForBlankPaymentId()
        {
            var service = CreateService(new OrderStore());

            Assert.IsNull(await service.GetByPaymentIdAsync(null));
            Assert.IsNull(await service.GetByPaymentIdAsync(" "));
        }

        [TestMethod]
        public void GetByPaymentId_IsCaseInsensitiveAndAttachesCustomer()
        {
            var orders = new OrderStore();
            orders.Orders.Add(new Order { Id = 9, PaymentId = "pay-ABC", UserId = "user-1" });
            var customers = new CustomerStore();
            customers.Customers.Add(new Customer { Id = 3, UserId = "user-1", Name = "Ada" });
            var service = CreateService(orders, customers: customers);

            var order = service.GetByPaymentId("pay-abc");

            Assert.IsNotNull(order);
            Assert.AreEqual(9, order.Id);
            Assert.IsNotNull(order.Customer);
            Assert.AreEqual("Ada", order.Customer.Name);
            CollectionAssert.Contains(customers.LookedUpUserIds, "user-1");
        }

        [TestMethod]
        public void GetByOrderGuid_DoesNotLookupCustomerWhenUserIdMissing()
        {
            var orders = new OrderStore();
            orders.Orders.Add(new Order { Id = 2, OrderGuid = "guid-1", UserId = "" });
            var customers = new CustomerStore();
            var service = CreateService(orders, customers: customers);

            var order = service.GetByOrderGuid("GUID-1");

            Assert.IsNotNull(order);
            Assert.AreEqual(0, customers.LookedUpUserIds.Count);
        }

        [TestMethod]
        public void DeleteOrderById_DeletesLineItemsThenOrphanedAddresses()
        {
            var orders = new OrderStore();
            orders.Orders.Add(new Order { Id = 11, ShippingAddressId = 100, BillingAddressId = 200, UserId = "u1" });
            var products = new OrderProductStore();
            var addresses = new AddressStore();
            var service = CreateService(orders, products, addresses);

            service.DeleteOrderById(11);

            CollectionAssert.AreEqual(new[] { 11 }, products.DeletedByOrderIds);
            CollectionAssert.AreEqual(new[] { 11 }, orders.DeletedOrderIds);
            CollectionAssert.Contains(addresses.DeletedAddressIds, 100);
            CollectionAssert.Contains(addresses.DeletedAddressIds, 200);
        }

        [TestMethod]
        public void DeleteOrderById_DoesNotDeleteAddressStillUsedByAnotherOrder()
        {
            var orders = new OrderStore();
            orders.Orders.Add(new Order { Id = 1, ShippingAddressId = 50, BillingAddressId = 50, UserId = "u1" });
            orders.Orders.Add(new Order { Id = 2, ShippingAddressId = 50, BillingAddressId = 60, UserId = "u2" });
            var products = new OrderProductStore();
            var addresses = new AddressStore();
            var service = CreateService(orders, products, addresses);

            service.DeleteOrderById(1);

            CollectionAssert.AreEqual(new[] { 1 }, orders.DeletedOrderIds);
            Assert.IsFalse(addresses.DeletedAddressIds.Contains(50), "Shared shipping address must remain for the surviving order.");
            Assert.IsFalse(addresses.DeletedAddressIds.Contains(60));
        }

        [TestMethod]
        public void DeleteOrderById_DeletesSharedShippingAndBillingAddressOnlyOnce()
        {
            var orders = new OrderStore();
            orders.Orders.Add(new Order { Id = 4, ShippingAddressId = 8, BillingAddressId = 8, UserId = "u1" });
            var addresses = new AddressStore();
            var service = CreateService(orders, new OrderProductStore(), addresses);

            service.DeleteOrderById(4);

            Assert.AreEqual(1, addresses.DeletedAddressIds.Count);
            Assert.AreEqual(8, addresses.DeletedAddressIds[0]);
        }

        [TestMethod]
        public async Task DeleteOrderByIdAsync_RemovesOrphanedAddresses()
        {
            var orders = new OrderStore();
            orders.Orders.Add(new Order { Id = 21, ShippingAddressId = 31, BillingAddressId = 32, UserId = "u1" });
            var products = new OrderProductStore();
            var addresses = new AddressStore();
            var service = CreateService(orders, products, addresses);

            await service.DeleteOrderByIdAsync(21);

            CollectionAssert.AreEqual(new[] { 21 }, products.DeletedByOrderIds);
            CollectionAssert.Contains(addresses.DeletedAddressIds, 31);
            CollectionAssert.Contains(addresses.DeletedAddressIds, 32);
        }

        [TestMethod]
        public void DeleteBaseEntity_IgnoresNullOrEmptyIds()
        {
            var products = new OrderProductStore();
            var service = CreateService(new OrderStore(), products);

            service.DeleteBaseEntity(null);
            service.DeleteBaseEntity(new List<string>());

            Assert.AreEqual(0, products.DeletedByOrderIds.Count);
        }

        [TestMethod]
        public void DeleteByUserId_DeletesEveryOrderForThatUser()
        {
            var orders = new OrderStore();
            orders.Orders.Add(new Order { Id = 1, UserId = "alice", ShippingAddressId = 1, BillingAddressId = 2 });
            orders.Orders.Add(new Order { Id = 2, UserId = "alice", ShippingAddressId = 3, BillingAddressId = 4 });
            orders.Orders.Add(new Order { Id = 3, UserId = "bob", ShippingAddressId = 5, BillingAddressId = 6 });
            var products = new OrderProductStore();
            var service = CreateService(orders, products, new AddressStore());

            service.DeleteByUserId("alice");

            CollectionAssert.AreEquivalent(new[] { 1, 2 }, orders.DeletedOrderIds);
            Assert.IsTrue(orders.Orders.Any(o => o.Id == 3));
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, products.DeletedByOrderIds);
        }
    }
}
