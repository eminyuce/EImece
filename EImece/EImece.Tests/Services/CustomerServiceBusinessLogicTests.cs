using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Enums;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class CustomerServiceBusinessLogicTests
    {
        private class CustomerStore
        {
            public List<Customer> Customers { get; } = new List<Customer>();
            public List<Customer> Saved { get; } = new List<Customer>();
            public List<string> DeletedUserIds { get; } = new List<string>();

            public int SaveOrEdit(Customer item)
            {
                if (item.Id == 0)
                {
                    item.Id = Saved.Count + 1;
                }
                Saved.Add(item);
                var existing = Customers.Find(c => c.UserId == item.UserId);
                if (existing != null)
                {
                    Customers.Remove(existing);
                }
                Customers.Add(item);
                return item.Id;
            }

            public Task<int> SaveOrEditAsync(Customer item)
            {
                return Task.FromResult(SaveOrEdit(item));
            }

            public Customer GetUserId(string userId)
            {
                return Customers.Find(c => c.UserId == userId);
            }

            public Task<Customer> GetUserIdAsync(string userId)
            {
                return Task.FromResult(GetUserId(userId));
            }

            public Task<Customer> GetUserIdAsync(string userId, System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult(GetUserId(userId));
            }

            public Task<bool> PromoteCustomerToNormalTypeAsync(string userId, int normalCustomerType)
            {
                var customer = GetUserId(userId);
                if (customer == null)
                {
                    return Task.FromResult(false);
                }
                customer.CustomerType = normalCustomerType;
                return Task.FromResult(true);
            }

            public Task<int> DeleteItemAsync(Customer entity)
            {
                Customers.Remove(entity);
                DeletedUserIds.Add(entity.UserId);
                return Task.FromResult(1);
            }

            public int DeleteItem(Customer entity)
            {
                Customers.Remove(entity);
                DeletedUserIds.Add(entity.UserId);
                return 1;
            }
        }

        private class OrderServiceStore
        {
            public List<string> DeletedUserIds { get; } = new List<string>();

            public void DeleteByUserId(string userId)
            {
                DeletedUserIds.Add(userId);
            }

            public Task DeleteByUserIdAsync(string userId)
            {
                DeletedUserIds.Add(userId);
                return Task.CompletedTask;
            }
        }

        private static CustomerService CreateService(CustomerStore store, OrderServiceStore orders = null)
        {
            var repo = new FakeServiceProxy<ICustomerRepository>(store).Instance;
            var orderService = orders == null ? null : new FakeServiceProxy<IOrderService>(orders).Instance;
            return new CustomerService(repo, TestNullLoggers.Create<CustomerService>(), null, null, orderService);
        }

        [TestMethod]
        public void Constructor_ThrowsWhenRepositoryIsNull()
        {
            try
            {
                var unused = new CustomerService(null, TestNullLoggers.Create<CustomerService>());
                Assert.Fail("Expected ArgumentNullException.");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [TestMethod]
        public void SaveRegisterViewModel_MapsNormalizedFieldsAndDefaultsCountry()
        {
            var store = new CustomerStore();
            var service = CreateService(store);

            service.SaveRegisterViewModel("user-9", new CustomerRegistrationDto
            {
                FirstName = "Emin",
                LastName = "Yilmaz",
                Email = "  emin@example.com ",
                PhoneNumber = "05321234567",
                IdentityNumber = " 12345678901 ",
                Street = "  Main ",
                District = "  Kadikoy ",
                Town = "  Moda ",
                City = "  Istanbul ",
                Country = "   ",
                ZipCode = " 34710 ",
                IsPermissionGranted = true
            });

            Assert.AreEqual(1, store.Saved.Count);
            var saved = store.Saved[0];
            Assert.AreEqual("user-9", saved.UserId);
            Assert.AreEqual("Emin", saved.Name);
            Assert.AreEqual("Yilmaz", saved.Surname);
            Assert.AreEqual("emin@example.com", saved.Email);
            Assert.AreEqual("+905321234567", saved.GsmNumber);
            Assert.AreEqual("12345678901", saved.IdentityNumber);
            Assert.AreEqual("Turkey", saved.Country);
            Assert.AreEqual("Main", saved.Street);
            Assert.AreEqual("Kadikoy", saved.District);
            Assert.AreEqual("Moda", saved.Town);
            Assert.AreEqual("Istanbul", saved.City);
            Assert.AreEqual("34710", saved.ZipCode);
            Assert.IsTrue(saved.IsActive);
            Assert.IsTrue(saved.IsPermissionGranted);
            Assert.AreEqual("85.34.78.112", saved.Ip);
        }

        [TestMethod]
        public void SaveRegisterViewModel_ThrowsWhenModelIsNull()
        {
            var service = CreateService(new CustomerStore());
            try
            {
                service.SaveRegisterViewModel("user-1", null);
                Assert.Fail("Expected ArgumentNullException.");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [TestMethod]
        public void SaveRegisterViewModel_ThrowsWhenPhoneNumberIsInvalid()
        {
            var service = CreateService(new CustomerStore());
            try
            {
                service.SaveRegisterViewModel("user-1", new CustomerRegistrationDto
                {
                    FirstName = "A",
                    LastName = "B",
                    PhoneNumber = "123"
                });
                Assert.Fail("Expected ArgumentException for invalid GSM.");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public async Task SaveRegisterViewModelAsync_WrapsPersistenceErrors()
        {
            var store = new CustomerStore();
            var service = CreateService(store);

            try
            {
                await service.SaveRegisterViewModelAsync("user-1", new CustomerRegistrationDto
                {
                    FirstName = "A",
                    LastName = "B",
                    PhoneNumber = "not-a-phone"
                });
                Assert.Fail("Expected InvalidOperationException.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsNotNull(ex.InnerException);
            }
        }

        [TestMethod]
        public void SaveCustomerTypeToNormal_PromotesExistingCustomer()
        {
            var store = new CustomerStore();
            store.Customers.Add(new Customer
            {
                Id = 5,
                UserId = "user-5",
                CustomerType = (int)EImeceCustomerType.ShoppingWithoutAccount,
                GsmNumber = "5321234567"
            });
            var service = CreateService(store);

            service.SaveCustomerTypeToNormal("user-5");

            Assert.AreEqual(1, store.Saved.Count);
            Assert.AreEqual((int)EImeceCustomerType.Normal, store.Saved[0].CustomerType);
            Assert.AreEqual("+905321234567", store.Saved[0].GsmNumber);
        }

        [TestMethod]
        public void SaveCustomerTypeToNormal_DoesNothingWhenCustomerMissing()
        {
            var store = new CustomerStore();
            var service = CreateService(store);

            service.SaveCustomerTypeToNormal("missing");

            Assert.AreEqual(0, store.Saved.Count);
        }

        [TestMethod]
        public async Task SaveCustomerTypeToNormalAsync_UsesTargetedPromotion()
        {
            var store = new CustomerStore();
            store.Customers.Add(new Customer
            {
                Id = 8,
                UserId = "user-8",
                CustomerType = (int)EImeceCustomerType.BuyNow,
                GsmNumber = "+905321234567"
            });
            var service = CreateService(store);

            await service.SaveCustomerTypeToNormalAsync("user-8");

            Assert.AreEqual((int)EImeceCustomerType.Normal, store.Customers[0].CustomerType);
        }

        [TestMethod]
        public async Task DeleteCustomersAsync_ReturnsEmptyForNullOrEmptyInput()
        {
            var service = CreateService(new CustomerStore());

            var empty = await service.DeleteCustomersAsync(null);
            var none = await service.DeleteCustomersAsync(new List<string>());

            Assert.AreEqual(0, empty.Count);
            Assert.AreEqual(0, none.Count);
        }

        [TestMethod]
        public async Task DeleteCustomersAsync_SkipsCurrentUserAndCascadesOrders()
        {
            var store = new CustomerStore();
            store.Customers.Add(new Customer { Id = 1, UserId = "self" });
            store.Customers.Add(new Customer { Id = 2, UserId = "other" });
            var orders = new OrderServiceStore();
            var service = CreateService(store, orders);

            var deleted = await service.DeleteCustomersAsync(new List<string> { "self", "other", "other", " " }, "self");

            Assert.AreEqual(1, deleted.Count, "Deleted ids: " + string.Join(",", deleted));
            Assert.AreEqual("other", deleted[0]);
            CollectionAssert.AreEqual(new[] { "other" }, store.DeletedUserIds);
            CollectionAssert.AreEqual(new[] { "other" }, orders.DeletedUserIds);
            Assert.IsTrue(store.Customers.Exists(c => c.UserId == "self"));
        }

        [TestMethod]
        public async Task DeleteByUserIdAsync_IsNoOpWhenCustomerDoesNotExist()
        {
            var store = new CustomerStore();
            var service = CreateService(store);

            await service.DeleteByUserIdAsync("ghost");

            Assert.AreEqual(0, store.DeletedUserIds.Count);
        }
    }
}
