using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Unit.Helpers
{
    [TestClass]
    public class ShoppingCartSessionCouponTests
    {
        [TestMethod]
        public void CalculateCouponDiscount_WhenFixedDiscount_ReturnsDiscountAmount()
        {
            var session = new ShoppingCartSession
            {
                Coupon = new Coupon { Discount = 25, DiscountPercentage = 0 }
            };

            Assert.AreEqual(25m, session.CalculateCouponDiscount(100m));
        }

        [TestMethod]
        public void CalculateCouponDiscount_WhenFixedDiscountExceedsTotal_ReturnsTotal()
        {
            var session = new ShoppingCartSession
            {
                Coupon = new Coupon { Discount = 80, DiscountPercentage = 0 }
            };

            Assert.AreEqual(50m, session.CalculateCouponDiscount(50m));
        }

        [TestMethod]
        public void CalculateCouponDiscount_WhenPercentage_ReturnsPercentOfTotal()
        {
            var session = new ShoppingCartSession
            {
                Coupon = new Coupon { Discount = 0, DiscountPercentage = 10 }
            };

            Assert.AreEqual(15m, session.CalculateCouponDiscount(150m));
        }

        [TestMethod]
        public void CalculateCouponDiscount_WhenNoCoupon_ReturnsZero()
        {
            var session = new ShoppingCartSession();
            Assert.AreEqual(0m, session.CalculateCouponDiscount(100m));
        }

        [TestMethod]
        public void TotalPrice_WhenEmptyCart_IsZero()
        {
            Assert.AreEqual(0m, new ShoppingCartSession().TotalPrice);
        }
    }
}
