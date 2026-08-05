using System.Linq;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories;
using EImece.Domain.Services;
using EImece.Integration.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Integration.Tests.Flows
{
    [TestClass]
    public class CartCouponOrderFlowTests
    {
        [TestMethod]
        public void ShoppingCart_SaveAndLoadByOrderGuid_RoundTrip()
        {
            if (!LegacyTestDatabase.CanConnect())
            {
                Assert.Inconclusive("LocalDB not available");
            }

            LegacyTestDatabase.RequireLocalDb();
            var guid = System.Guid.NewGuid().ToString();

            using (var db = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var svc = new ShoppingCartService(
                    null,
                    new ShoppingCartRepository(db),
                    Moq.Mock.Of<EImece.Domain.Services.IServices.IOrderService>(),
                    Moq.Mock.Of<EImece.Domain.Services.IServices.ICustomerService>(),
                    Moq.Mock.Of<EImece.Domain.Services.IServices.IAddressService>(),
                    Moq.Mock.Of<EImece.Domain.Services.IServices.IOrderProductService>());

                svc.SaveOrEditShoppingCart(new ShoppingCart
                {
                    Name = "IT-Cart",
                    OrderGuid = guid,
                    ShoppingCartJson = "{\"items\":1}",
                    IsActive = true,
                    Lang = 1,
                    CreatedDate = System.DateTime.UtcNow,
                    UpdatedDate = System.DateTime.UtcNow
                });

                var loaded = svc.GetShoppingCartByOrderGuid(guid);
                Assert.IsNotNull(loaded);
                Assert.AreEqual("{\"items\":1}", loaded.ShoppingCartJson);
            }
        }

        [TestMethod]
        public void Coupon_GetByCode_FromSeededLocalDb()
        {
            if (!LegacyTestDatabase.CanConnect())
            {
                Assert.Inconclusive("LocalDB not available");
            }

            LegacyTestDatabase.RequireLocalDb();
            var code = "IT" + System.Guid.NewGuid().ToString("N").Substring(0, 6);

            using (var db = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                db.Coupons.Add(new Coupon
                {
                    Name = "IT Coupon",
                    Code = code,
                    Discount = 15,
                    DiscountPercentage = 0,
                    IsActive = true,
                    Lang = 1,
                    CreatedDate = System.DateTime.UtcNow,
                    UpdatedDate = System.DateTime.UtcNow
                });
                db.SaveChanges();
            }

            using (var db = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var svc = new CouponService(new CouponRepository(db));
                var coupon = svc.GetCouponByCode(code, 1);
                Assert.IsNotNull(coupon);

                var session = new ShoppingCartSession { Coupon = coupon };
                Assert.AreEqual(15m, session.CalculateCouponDiscount(100m));
            }
        }

        [TestMethod]
        public void BrandOrdering_PersistsViaBrandService()
        {
            if (!LegacyTestDatabase.CanConnect())
            {
                Assert.Inconclusive("LocalDB not available");
            }

            var seed = LegacyTestDatabase.SeedMinimalCatalog();
            using (var db = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var svc = new BrandService(new BrandRepository(db));
                svc.ChangeGridBaseEntityOrderingOrState(
                    new System.Collections.Generic.List<EImece.Domain.Models.HelperModels.OrderingItem>
                    {
                        new EImece.Domain.Models.HelperModels.OrderingItem
                        {
                            Id = seed.BrandId,
                            Position = 7,
                            IsActive = true
                        }
                    },
                    "State");
            }

            using (var verify = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var brand = verify.Brands.Single(b => b.Id == seed.BrandId);
                Assert.IsTrue(brand.IsActive);
            }
        }
    }
}
