using EImece.Domain.Entities;
using EImece.Domain.Models;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Repositories;
using EImece.Domain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    public class FakeDbSet<T> : IDbSet<T>, System.Data.Entity.Infrastructure.IDbAsyncEnumerable<T> where T : class
    {
        private readonly List<T> _data;
        private readonly IQueryable<T> _query;
        public FakeDbSet() { _data = new List<T>(); _query = _data.AsQueryable(); }
        public FakeDbSet(IEnumerable<T> data) { _data = data.ToList(); _query = _data.AsQueryable(); }
        public T Add(T entity) { _data.Add(entity); return entity; }
        public T Attach(T entity) { if (!_data.Contains(entity)) _data.Add(entity); return entity; }
        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T => Activator.CreateInstance<TDerivedEntity>();
        public T Create() => Activator.CreateInstance<T>();
        public T Find(params object[] keyValues) => _data.FirstOrDefault();
        public System.Collections.ObjectModel.ObservableCollection<T> Local => new System.Collections.ObjectModel.ObservableCollection<T>(_data);
        public T Remove(T entity) { _data.Remove(entity); return entity; }
        public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _data.GetEnumerator();
        public Type ElementType => _query.ElementType;
        public System.Linq.Expressions.Expression Expression => _query.Expression;
        public IQueryProvider Provider => new FakeAsyncQueryProvider<T>(_query.Provider);
        public System.Data.Entity.Infrastructure.IDbAsyncEnumerator<T> GetAsyncEnumerator() => new FakeAsyncEnumerator<T>(_data.GetEnumerator());
        System.Data.Entity.Infrastructure.IDbAsyncEnumerator System.Data.Entity.Infrastructure.IDbAsyncEnumerable.GetAsyncEnumerator() => GetAsyncEnumerator();
        public List<T> Data => _data;
    }
    public class FakeAsyncQueryProvider<TEntity> : System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        public FakeAsyncQueryProvider(IQueryProvider inner) { _inner = inner; }
        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression) => new FakeAsyncEnumerable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression) => new FakeAsyncEnumerable<TElement>(expression);
        public object Execute(System.Linq.Expressions.Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) => _inner.Execute<TResult>(expression);
        public Task<object> ExecuteAsync(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken) => Task.FromResult(Execute(expression));
        public Task<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken) => Task.FromResult(Execute<TResult>(expression));
    }
    public class FakeAsyncEnumerable<T> : EnumerableQuery<T>, System.Data.Entity.Infrastructure.IDbAsyncEnumerable<T>, IQueryable<T>
    {
        public FakeAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public FakeAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }
        public System.Data.Entity.Infrastructure.IDbAsyncEnumerator<T> GetAsyncEnumerator() => new FakeAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        System.Data.Entity.Infrastructure.IDbAsyncEnumerator System.Data.Entity.Infrastructure.IDbAsyncEnumerable.GetAsyncEnumerator() => GetAsyncEnumerator();
        IQueryProvider IQueryable.Provider => new FakeAsyncQueryProvider<T>(this);
    }
    public class FakeAsyncEnumerator<T> : System.Data.Entity.Infrastructure.IDbAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public FakeAsyncEnumerator(IEnumerator<T> inner) { _inner = inner; }
        public void Dispose() => _inner.Dispose();
        public Task<bool> MoveNextAsync(CancellationToken cancellationToken) => Task.FromResult(_inner.MoveNext());
        public T Current => _inner.Current;
        object System.Data.Entity.Infrastructure.IDbAsyncEnumerator.Current => Current;
    }

    public class FakeEImeceContext : EImece.Domain.DbContext.IEImeceContext
    {
        public FakeEImeceContext()
        {
            Coupons = new FakeDbSet<Coupon>();
            CouponProducts = new FakeDbSet<CouponProduct>();
            CouponCategories = new FakeDbSet<CouponCategory>();
            CouponRedemptions = new FakeDbSet<CouponRedemption>();
            Products = new FakeDbSet<Product>();
            ProductCategories = new FakeDbSet<ProductCategory>();
            ProductTags = new FakeDbSet<ProductTag>();
            Customers = new FakeDbSet<Customer>();
            Orders = new FakeDbSet<Order>();
            Addresses = new FakeDbSet<Address>();
            ShoppingCarts = new FakeDbSet<ShoppingCart>();
            OrderProducts = new FakeDbSet<OrderProduct>();
            Settings = new FakeDbSet<Setting>();
            Brands = new FakeDbSet<Brand>();
            MailTemplates = new FakeDbSet<EImece.Domain.Entities.MailTemplate>();
            ListItems = new FakeDbSet<EImece.Domain.Entities.ListItem>();
            Lists = new FakeDbSet<EImece.Domain.Entities.List>();
            Menus = new FakeDbSet<Menu>();
            Tags = new FakeDbSet<Tag>();
            TagCategories = new FakeDbSet<TagCategory>();
            Subscribers = new FakeDbSet<Subscriber>();
            Stories = new FakeDbSet<Story>();
            StoryCategories = new FakeDbSet<StoryCategory>();
            StoryFiles = new FakeDbSet<StoryFile>();
            StoryTags = new FakeDbSet<StoryTag>();
            ProductSpecifications = new FakeDbSet<ProductSpecification>();
            FileStorages = new FakeDbSet<FileStorage>();
            FileStorageTags = new FakeDbSet<FileStorageTag>();
            Templates = new FakeDbSet<Template>();
            MenuFiles = new FakeDbSet<MenuFile>();
            BrowserSubscribers = new FakeDbSet<BrowserSubscriber>();
            BrowserSubscriptions = new FakeDbSet<BrowserSubscription>();
            BrowserNotificationFeedBacks = new FakeDbSet<BrowserNotificationFeedBack>();
            BrowserNotifications = new FakeDbSet<BrowserNotification>();
            Faqs = new FakeDbSet<Faq>();
            ProductComments = new FakeDbSet<ProductComment>();
            ProductFiles = new FakeDbSet<ProductFile>();
            MainPageImages = new FakeDbSet<MainPageImage>();
        }
        public System.Data.Entity.Infrastructure.DbEntityEntry Entry(object entity) => null;
        public System.Data.Entity.Infrastructure.DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => null;
        public int SaveChanges() => 0;
        public Task<int> SaveChangesAsync() => Task.FromResult(0);
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(0);
        public void Dispose() {}
        public IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            if (typeof(TEntity) == typeof(Coupon)) return (IDbSet<TEntity>)(object)Coupons;
            if (typeof(TEntity) == typeof(CouponProduct)) return (IDbSet<TEntity>)(object)CouponProducts;
            if (typeof(TEntity) == typeof(CouponCategory)) return (IDbSet<TEntity>)(object)CouponCategories;
            if (typeof(TEntity) == typeof(CouponRedemption)) return (IDbSet<TEntity>)(object)CouponRedemptions;
            if (typeof(TEntity) == typeof(Product)) return (IDbSet<TEntity>)(object)Products;
            if (typeof(TEntity) == typeof(ProductCategory)) return (IDbSet<TEntity>)(object)ProductCategories;
            if (typeof(TEntity) == typeof(Order)) return (IDbSet<TEntity>)(object)Orders;
            if (typeof(TEntity) == typeof(Customer)) return (IDbSet<TEntity>)(object)Customers;
            throw new NotImplementedException();
        }
        public void SetAsAdded<TEntity>(TEntity entity) where TEntity : class {}
        public void SetAsModified<TEntity>(TEntity entity) where TEntity : class {}
        public void SetAsDeleted<TEntity>(TEntity entity) where TEntity : class {}
        public IDbSet<EImece.Domain.Entities.MailTemplate> MailTemplates { get; set; }
        public IDbSet<EImece.Domain.Entities.ListItem> ListItems { get; set; }
        public IDbSet<EImece.Domain.Entities.List> Lists { get; set; }
        public IDbSet<Product> Products { get; set; }
        public IDbSet<ProductTag> ProductTags { get; set; }
        public IDbSet<ProductFile> ProductFiles { get; set; }
        public IDbSet<ProductCategory> ProductCategories { get; set; }
        public IDbSet<Menu> Menus { get; set; }
        public IDbSet<Tag> Tags { get; set; }
        public IDbSet<TagCategory> TagCategories { get; set; }
        public IDbSet<Subscriber> Subscribers { get; set; }
        public IDbSet<Story> Stories { get; set; }
        public IDbSet<StoryCategory> StoryCategories { get; set; }
        public IDbSet<StoryFile> StoryFiles { get; set; }
        public IDbSet<StoryTag> StoryTags { get; set; }
        public IDbSet<ProductSpecification> ProductSpecifications { get; set; }
        public IDbSet<FileStorage> FileStorages { get; set; }
        public IDbSet<FileStorageTag> FileStorageTags { get; set; }
        public IDbSet<Setting> Settings { get; set; }
        public IDbSet<Template> Templates { get; set; }
        public IDbSet<MenuFile> MenuFiles { get; set; }
        public IDbSet<BrowserSubscriber> BrowserSubscribers { get; set; }
        public IDbSet<BrowserSubscription> BrowserSubscriptions { get; set; }
        public IDbSet<BrowserNotificationFeedBack> BrowserNotificationFeedBacks { get; set; }
        public IDbSet<BrowserNotification> BrowserNotifications { get; set; }
        public IDbSet<Customer> Customers { get; set; }
        public IDbSet<Address> Addresses { get; set; }
        public IDbSet<ShoppingCart> ShoppingCarts { get; set; }
        public IDbSet<Order> Orders { get; set; }
        public IDbSet<OrderProduct> OrderProducts { get; set; }
        public IDbSet<Faq> Faqs { get; set; }
        public IDbSet<ProductComment> ProductComments { get; set; }
        public IDbSet<Brand> Brands { get; set; }
        public IDbSet<Coupon> Coupons { get; set; }
        public IDbSet<CouponProduct> CouponProducts { get; set; }
        public IDbSet<CouponCategory> CouponCategories { get; set; }
        public IDbSet<CouponRedemption> CouponRedemptions { get; set; }
        public IDbSet<MainPageImage> MainPageImages { get; set; }
    }

    [TestClass]
    public class CouponValidationServiceTests
    {
        private FakeEImeceContext _ctx;
        private CouponValidationService _service;

        private Coupon CreateBaseCoupon(string code, int discount = 0, int discountPct = 0, CouponDiscountType type = CouponDiscountType.FixedAmount)
        {
            return new Coupon
            {
                Id = new Random().Next(1000, 9999),
                Name = "Test " + code,
                Code = code,
                IsActive = true,
                Lang = 1,
                Position = 0,
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(10),
                Discount = discount,
                DiscountPercentage = discountPct,
                DiscountType = type,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                CouponProducts = new List<CouponProduct>(),
                CouponCategories = new List<CouponCategory>()
            };
        }

        private ShoppingCartSession CreateCart(params (int id, decimal price, int qty)[] items)
        {
            var cart = new ShoppingCartSession { CurrentLanguage = 1 };
            cart.ShoppingCartItems = items.Select(x => new ShoppingCartItem
            {
                Quantity = x.qty,
                Product = new ShoppingCartProduct { Id = x.id, Name = "P" + x.id, Price = x.price }
            }).ToList();
            cart.CargoPrice = new EImece.Domain.Models.DTOs.Storefront.SettingValueDto { SettingValue = "20" };
            cart.BasketMinTotalPriceForCargo = new EImece.Domain.Models.DTOs.Storefront.SettingValueDto { SettingValue = "500" };
            return cart;
        }

        [TestInitialize]
        public void Setup()
        {
            _ctx = new FakeEImeceContext();
            var couponRepo = new CouponRepository(_ctx);
            var couponProductRepo = new CouponProductRepository(_ctx);
            var couponCategoryRepo = new CouponCategoryRepository(_ctx);
            var productRepo = new ProductRepository(_ctx);
            var productCategoryRepo = new ProductCategoryRepository(_ctx);
            var orderRepo = new OrderRepository(_ctx);
            var redemptionRepo = new CouponRedemptionRepository(_ctx);
            _service = new CouponValidationService(couponRepo, redemptionRepo, couponProductRepo, couponCategoryRepo, productRepo, productCategoryRepo, orderRepo);
            _ctx.ProductCategories.Add(new ProductCategory { Id = 1, Name = "Shoes", DiscountPercantage = 0 });
            _ctx.ProductCategories.Add(new ProductCategory { Id = 2, Name = "SaleCat", DiscountPercantage = 20 });
            _ctx.Products.Add(new Product { Id = 1, Name = "Shoe A", Price = 100, ProductCategoryId = 1, State = "ProductInStock" });
            _ctx.Products.Add(new Product { Id = 2, Name = "Prod B", Price = 200, ProductCategoryId = 1, State = "ProductInStock", Discount = 0 });
            _ctx.Products.Add(new Product { Id = 3, Name = "Sale Item", Price = 150, ProductCategoryId = 1, State = "ProductInStock", Discount = 30 });
        }

        [TestMethod]
        public async Task ValidCoupon_ShouldSucceed()
        {
            var coupon = CreateBaseCoupon("SAVE10", discount: 50);
            _ctx.Coupons.Add(coupon);
            var cart = CreateCart((1, 200, 2));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1, CargoPrice = 20 };
            var result = await _service.ValidateCouponAsync("SAVE10", cart, ctx);
            Assert.IsTrue(result.IsValid, result.Message);
            Assert.AreEqual(50, result.DiscountAmount);
        }

        [TestMethod]
        public async Task InactiveCoupon_ShouldFail()
        {
            var coupon = CreateBaseCoupon("INACTIVE", 10);
            coupon.IsActive = false;
            _ctx.Coupons.Add(coupon);
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("INACTIVE", cart, ctx);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(CouponValidationReason.CouponInactive, result.Reason);
        }

        [TestMethod]
        public async Task ExpiredCoupon_ShouldFail()
        {
            var coupon = CreateBaseCoupon("EXPIRED", 10);
            coupon.EndDate = DateTime.Now.AddDays(-1);
            _ctx.Coupons.Add(coupon);
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("EXPIRED", cart, ctx);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(CouponValidationReason.CouponExpired, result.Reason);
        }

        [TestMethod]
        public async Task FutureCoupon_ShouldFail()
        {
            var coupon = CreateBaseCoupon("FUTURE", 10);
            coupon.StartDate = DateTime.Now.AddDays(2);
            coupon.EndDate = DateTime.Now.AddDays(5);
            _ctx.Coupons.Add(coupon);
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("FUTURE", cart, ctx);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(CouponValidationReason.CouponNotYetValid, result.Reason);
        }

        [TestMethod]
        public async Task GlobalUsageLimit_ShouldFailWhenReached()
        {
            var coupon = CreateBaseCoupon("LIMIT1", 10);
            coupon.GlobalUsageLimit = 1;
            _ctx.Coupons.Add(coupon);
            _ctx.CouponRedemptions.Add(new CouponRedemption { CouponId = coupon.Id, UserId = "other", CouponCode = coupon.Code, Name = coupon.Code });
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u2", Language = 1 };
            var result = await _service.ValidateCouponAsync("LIMIT1", cart, ctx);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(CouponValidationReason.UsageLimitReached, result.Reason);
        }

        [TestMethod]
        public async Task PerCustomerLimit_OneTime_ShouldFailOnSecondUse()
        {
            var coupon = CreateBaseCoupon("ONCE", 10);
            coupon.PerCustomerUsageLimit = 1;
            coupon.RequireLogin = true;
            _ctx.Coupons.Add(coupon);
            _ctx.CouponRedemptions.Add(new CouponRedemption { CouponId = coupon.Id, UserId = "u1", CouponCode = coupon.Code, Name = coupon.Code });
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("ONCE", cart, ctx);
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Reason == CouponValidationReason.AlreadyUsedByCustomer || result.Reason == CouponValidationReason.CustomerUsageLimitReached);
        }

        [TestMethod]
        public async Task MinOrderAmount_NotMet_ShouldFail()
        {
            var coupon = CreateBaseCoupon("MIN500", 50);
            coupon.MinimumOrderAmount = 500;
            _ctx.Coupons.Add(coupon);
            var cart = CreateCart((1, 100, 2));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("MIN500", cart, ctx);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(CouponValidationReason.MinOrderAmountNotMet, result.Reason);
        }

        [TestMethod]
        public async Task ProductRestriction_NotApplicable_ShouldFail()
        {
            var coupon = CreateBaseCoupon("PRODONLY", 20);
            coupon.CouponProducts.Add(new CouponProduct { CouponId = coupon.Id, ProductId = 99 });
            _ctx.Coupons.Add(coupon);
            _ctx.CouponProducts.Add(new CouponProduct { CouponId = coupon.Id, ProductId = 99 });
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("PRODONLY", cart, ctx);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(CouponValidationReason.NotApplicableToCartItems, result.Reason);
        }

        [TestMethod]
        public async Task ProductRestriction_Eligible_ShouldDiscountOnlyEligible()
        {
            var coupon = CreateBaseCoupon("PROD10", 10);
            coupon.DiscountType = CouponDiscountType.Percentage;
            coupon.DiscountPercentage = 10;
            coupon.CouponProducts.Add(new CouponProduct { CouponId = coupon.Id, ProductId = 1 });
            _ctx.Coupons.Add(coupon);
            _ctx.CouponProducts.Add(new CouponProduct { CouponId = coupon.Id, ProductId = 1 });
            var cart = CreateCart((1, 100, 2), (2, 200, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("PROD10", cart, ctx);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(20, result.DiscountAmount);
            Assert.AreEqual(200, result.EligibleAmount);
        }

        [TestMethod]
        public async Task CategoryRestriction_ShouldWork()
        {
            var coupon = CreateBaseCoupon("CAT10", 10);
            coupon.CouponCategories.Add(new CouponCategory { CouponId = coupon.Id, ProductCategoryId = 1 });
            _ctx.Coupons.Add(coupon);
            _ctx.CouponCategories.Add(new CouponCategory { CouponId = coupon.Id, ProductCategoryId = 1 });
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("CAT10", cart, ctx);
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public async Task SaleExclusion_ShouldExcludeSaleItems()
        {
            var coupon = CreateBaseCoupon("NOSALE", 10);
            coupon.ExcludeSaleItems = true;
            _ctx.Coupons.Add(coupon);
            var cartEligible = CreateCart((1, 100, 1));
            var cartSaleOnly = CreateCart((3, 150, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var r1 = await _service.ValidateCouponAsync("NOSALE", cartEligible, ctx);
            Assert.IsTrue(r1.IsValid);
            var r2 = await _service.ValidateCouponAsync("NOSALE", cartSaleOnly, ctx);
            Assert.IsFalse(r2.IsValid);
            Assert.AreEqual(CouponValidationReason.NotApplicableToCartItems, r2.Reason);
        }

        [TestMethod]
        public async Task Percentage_MaxCap_ShouldCap()
        {
            var coupon = CreateBaseCoupon("PCT20", discountPct: 20, type: CouponDiscountType.Percentage);
            coupon.DiscountPercentage = 20;
            coupon.DiscountType = CouponDiscountType.Percentage;
            coupon.MaximumDiscountAmount = 30;
            _ctx.Coupons.Add(coupon);
            var cart = CreateCart((1, 1000, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("PCT20", cart, ctx);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(30, result.DiscountAmount);
        }

        [TestMethod]
        public async Task FixedAmount_NeverExceedEligible()
        {
            var coupon = CreateBaseCoupon("FIX100", discount: 100);
            coupon.DiscountType = CouponDiscountType.FixedAmount;
            _ctx.Coupons.Add(coupon);
            var cart = CreateCart((1, 30, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("FIX100", cart, ctx);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(30, result.DiscountAmount);
        }

        [TestMethod]
        public async Task FreeShipping_ShouldGiveShippingDiscount()
        {
            var coupon = CreateBaseCoupon("FREESHIP", 0);
            coupon.DiscountType = CouponDiscountType.FreeShipping;
            coupon.IsFreeShipping = true;
            _ctx.Coupons.Add(coupon);
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1, CargoPrice = 25 };
            var result = await _service.ValidateCouponAsync("FREESHIP", cart, ctx);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(25, result.ShippingDiscount);
            Assert.AreEqual(0, result.DiscountAmount);
        }

        [TestMethod]
        public async Task Stacking_NotAllowed_ShouldFail()
        {
            var coupon1 = CreateBaseCoupon("FIRST", 10);
            var coupon2 = CreateBaseCoupon("SECOND", 10);
            _ctx.Coupons.Add(coupon1);
            _ctx.Coupons.Add(coupon2);
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1, HasExistingCoupon = true, ExistingCouponCode = "FIRST" };
            var result = await _service.ValidateCouponAsync("SECOND", cart, ctx);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(CouponValidationReason.StackingNotAllowed, result.Reason);
        }

        [TestMethod]
        public async Task Birthday_Eligible_ShouldPass()
        {
            var coupon = CreateBaseCoupon("BDAY", 10);
            coupon.IsBirthdayCoupon = true;
            coupon.BirthdayWindow = CouponBirthdayWindow.Month;
            _ctx.Coupons.Add(coupon);
            var cart = CreateCart((1, 100, 1));
            var birth = new DateTime(1990, DateTime.Today.Month, 10);
            var ctxEligible = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1, BirthDate = birth };
            var result = await _service.ValidateCouponAsync("BDAY", cart, ctxEligible);
            Assert.IsTrue(result.IsValid);
            var birthWrong = new DateTime(1990, DateTime.Today.AddMonths(1).Month, 10);
            if (DateTime.Today.Month == 12) birthWrong = new DateTime(1990, 1, 10);
            var ctxNotEligible = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1, BirthDate = birthWrong };
            var result2 = await _service.ValidateCouponAsync("BDAY", cart, ctxNotEligible);
            if (birthWrong.Month != DateTime.Today.Month)
                Assert.IsFalse(result2.IsValid);
        }

        [TestMethod]
        public async Task LoginRequired_Guest_ShouldFail()
        {
            var coupon = CreateBaseCoupon("LOGINREQ", 10);
            coupon.RequireLogin = true;
            _ctx.Coupons.Add(coupon);
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = false, UserId = null, Language = 1 };
            var result = await _service.ValidateCouponAsync("LOGINREQ", cart, ctx);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(CouponValidationReason.LoginRequired, result.Reason);
        }

        [TestMethod]
        public async Task FirstOrder_AlreadyHasOrder_ShouldFail()
        {
            var coupon = CreateBaseCoupon("FIRSTONLY", 10);
            coupon.IsFirstOrderOnly = true;
            _ctx.Coupons.Add(coupon);
            _ctx.Orders.Add(new Order { UserId = "u1", OrderStatus = (int)EImeceOrderStatus.NewlyOrder });
            var cart = CreateCart((1, 100, 1));
            var ctx = new CouponValidationContext { IsAuthenticated = true, UserId = "u1", Language = 1 };
            var result = await _service.ValidateCouponAsync("FIRSTONLY", cart, ctx);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(CouponValidationReason.FirstOrderOnly, result.Reason);
        }
    }
}
