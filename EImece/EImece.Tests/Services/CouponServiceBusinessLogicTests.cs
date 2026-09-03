using EImece.Domain.Entities;
using EImece.Domain.Repositories;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
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
    public class CouponServiceBusinessLogicTests
    {
        private class RestrictionStore<T> where T : class
        {
            public List<T> Items { get; } = new List<T>();
            public List<T> Deleted { get; } = new List<T>();
            public List<T> Added { get; } = new List<T>();

            public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
            {
                return new FakeAsyncEnumerable<T>(Items.Where(predicate.Compile()));
            }

            public void Delete(T entity)
            {
                Deleted.Add(entity);
                Items.Remove(entity);
            }

            public void Add(T entity)
            {
                Added.Add(entity);
                Items.Add(entity);
            }

            public Task<int> SaveAsync()
            {
                return Task.FromResult(Items.Count);
            }
        }

        private static CouponService CreateService(
            RestrictionStore<CouponProduct> products,
            RestrictionStore<CouponCategory> categories,
            FakeEImeceContext context = null)
        {
            var ctx = context ?? new FakeEImeceContext();
            return new CouponService(
                new CouponRepository(ctx, TestNullLoggers.Create<CouponRepository>()),
                new FakeServiceProxy<ICouponProductRepository>(products).Instance,
                new FakeServiceProxy<ICouponCategoryRepository>(categories).Instance,
                new CouponRedemptionRepository(ctx),
                new OrderRepository(ctx, TestNullLoggers.Create<OrderRepository>()),
                new CustomerRepository(ctx, TestNullLoggers.Create<CustomerRepository>()),
                TestNullLoggers.Create<CouponService>());
        }

        [TestMethod]
        public void Constructor_ThrowsWhenRequiredRepositoriesAreNull()
        {
            var ctx = new FakeEImeceContext();
            var couponRepo = new CouponRepository(ctx, TestNullLoggers.Create<CouponRepository>());
            var productRepo = new CouponProductRepository(ctx);
            var categoryRepo = new CouponCategoryRepository(ctx);
            var redemptionRepo = new CouponRedemptionRepository(ctx);
            var orderRepo = new OrderRepository(ctx, TestNullLoggers.Create<OrderRepository>());
            var customerRepo = new CustomerRepository(ctx, TestNullLoggers.Create<CustomerRepository>());
            var logger = TestNullLoggers.Create<CouponService>();

            try
            {
                var unused = new CouponService(null, productRepo, categoryRepo, redemptionRepo, orderRepo, customerRepo, logger);
                Assert.Fail("Expected ArgumentNullException.");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [TestMethod]
        public async Task SaveCouponRestrictionsAsync_ReplacesPreviousLinksAndIgnoresInvalidIds()
        {
            var products = new RestrictionStore<CouponProduct>();
            products.Items.Add(new CouponProduct { CouponId = 5, ProductId = 99 });
            var categories = new RestrictionStore<CouponCategory>();
            categories.Items.Add(new CouponCategory { CouponId = 5, ProductCategoryId = 77 });
            var service = CreateService(products, categories);

            await service.SaveCouponRestrictionsAsync(5, "1, 2, 2, 0, abc, 3", "10;10;0;8");

            Assert.AreEqual(1, products.Deleted.Count);
            Assert.AreEqual(1, categories.Deleted.Count);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, products.Added.Select(p => p.ProductId).ToList());
            CollectionAssert.AreEquivalent(new[] { 10, 8 }, categories.Added.Select(c => c.ProductCategoryId).ToList());
            Assert.IsTrue(products.Added.All(p => p.CouponId == 5));
            Assert.IsTrue(categories.Added.All(c => c.CouponId == 5));
        }

        [TestMethod]
        public async Task SaveCouponRestrictionsAsync_ClearsRestrictionsWhenCsvIsEmpty()
        {
            var products = new RestrictionStore<CouponProduct>();
            products.Items.Add(new CouponProduct { CouponId = 3, ProductId = 1 });
            var categories = new RestrictionStore<CouponCategory>();
            categories.Items.Add(new CouponCategory { CouponId = 3, ProductCategoryId = 4 });
            var service = CreateService(products, categories);

            await service.SaveCouponRestrictionsAsync(3, " ", "");

            Assert.AreEqual(0, products.Items.Count);
            Assert.AreEqual(0, categories.Items.Count);
            Assert.AreEqual(0, products.Added.Count);
            Assert.AreEqual(0, categories.Added.Count);
        }

        [TestMethod]
        public async Task GetRedemptionsWithDetailsAsync_JoinsOrderAndCustomerNames()
        {
            var ctx = new FakeEImeceContext();
            ctx.Orders.Add(new Order { Id = 40, OrderNumber = "ORD-40" });
            ctx.Customers.Add(new Customer { Id = 7, Name = "Ada", Surname = "Lovelace" });
            ctx.CouponRedemptions.Add(new CouponRedemption
            {
                Id = 1,
                CouponId = 12,
                CouponCode = "SAVE10",
                OrderId = 40,
                CustomerId = 7,
                UserId = "u1",
                DiscountAmount = 15,
                CreatedDate = DateTime.Now.AddMinutes(-5),
                Name = "SAVE10"
            });
            ctx.CouponRedemptions.Add(new CouponRedemption
            {
                Id = 2,
                CouponId = 12,
                CouponCode = "SAVE10",
                OrderId = 999,
                CustomerId = null,
                UserId = "guest",
                DiscountAmount = 5,
                CreatedDate = DateTime.Now,
                Name = "SAVE10"
            });
            ctx.CouponRedemptions.Add(new CouponRedemption
            {
                Id = 3,
                CouponId = 99,
                CouponCode = "OTHER",
                OrderId = 1,
                Name = "OTHER",
                CreatedDate = DateTime.Now
            });

            var service = CreateService(new RestrictionStore<CouponProduct>(), new RestrictionStore<CouponCategory>(), ctx);

            var details = await service.GetRedemptionsWithDetailsAsync(12, 10);
            var count = await service.GetRedemptionCountAsync(12);

            Assert.AreEqual(2, count);
            Assert.AreEqual(2, details.Count);
            Assert.AreEqual(2, details[0].Id, "Newest redemption should be first.");
            Assert.AreEqual("", details[0].OrderNumber);
            Assert.AreEqual("", details[0].CustomerName);
            Assert.AreEqual("ORD-40", details[1].OrderNumber);
            Assert.AreEqual("Ada Lovelace", details[1].CustomerName);
            Assert.AreEqual(15m, details[1].DiscountAmount);
        }

        [TestMethod]
        public async Task GetRedemptionCountAsync_ReturnsZeroWhenCouponHasNoHistory()
        {
            var service = CreateService(new RestrictionStore<CouponProduct>(), new RestrictionStore<CouponCategory>(), new FakeEImeceContext());

            Assert.AreEqual(0, await service.GetRedemptionCountAsync(1));
            var recent = await service.GetRecentRedemptionsAsync(1, 5);
            Assert.AreEqual(0, recent.Count);
        }
    }
}
