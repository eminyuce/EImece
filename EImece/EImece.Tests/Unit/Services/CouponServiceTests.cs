using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EImece.Tests.Unit.Services
{
    [TestClass]
    public class CouponServiceTests
    {
        [TestMethod]
        public void GetCouponByCode_DelegatesToRepository()
        {
            var coupon = new Coupon { Id = 1, Code = "SAVE10", Name = "Save" };
            var repo = new Mock<ICouponRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetCouponByCode("SAVE10", 1)).Returns(coupon);

            var sut = new CouponService(repo.Object);
            var result = sut.GetCouponByCode("SAVE10", 1);

            Assert.AreSame(coupon, result);
            repo.VerifyAll();
        }
    }
}
