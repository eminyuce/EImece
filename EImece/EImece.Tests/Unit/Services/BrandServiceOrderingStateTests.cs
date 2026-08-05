using System.Collections.Generic;
using EImece.Domain.Entities;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EImece.Tests.Unit.Services
{
    [TestClass]
    public class BrandServiceOrderingStateTests
    {
        private Mock<IBrandRepository> _repo;
        private BrandService _sut;
        private Brand _brand;

        [TestInitialize]
        public void Init()
        {
            _brand = new Brand
            {
                Id = 5,
                Name = "Test Brand",
                Position = 0,
                IsActive = false,
                MainPage = false
            };
            _repo = new Mock<IBrandRepository>(MockBehavior.Strict);
            _repo.Setup(r => r.GetSingle(5)).Returns(_brand);
            _repo.Setup(r => r.Edit(It.IsAny<Brand>()));
            _repo.Setup(r => r.Save()).Returns(1);
            _sut = new BrandService(_repo.Object);
        }

        [TestMethod]
        public void ChangeGridBaseEntityOrderingOrState_WhenNoCheckbox_UpdatesPosition()
        {
            var values = new List<OrderingItem>
            {
                new OrderingItem { Id = 5, Position = 12, IsActive = false }
            };

            _sut.ChangeGridBaseEntityOrderingOrState(values, "");

            Assert.AreEqual(12, _brand.Position);
            _repo.Verify(r => r.Edit(_brand), Times.Once);
            _repo.Verify(r => r.Save(), Times.Once);
        }

        [TestMethod]
        public void ChangeGridBaseEntityOrderingOrState_WhenState_UpdatesIsActive()
        {
            var values = new List<OrderingItem>
            {
                new OrderingItem { Id = 5, Position = 0, IsActive = true }
            };

            _sut.ChangeGridBaseEntityOrderingOrState(values, "State");

            Assert.IsTrue(_brand.IsActive);
        }

        [TestMethod]
        public void ChangeGridBaseEntityOrderingOrState_WhenMainPage_OnBrand_LeavesMainPageUnchanged()
        {
            // BaseEntityService applies MainPage only to Product/Story/ProductCategory.
            var values = new List<OrderingItem>
            {
                new OrderingItem { Id = 5, Position = 0, IsActive = true }
            };

            _sut.ChangeGridBaseEntityOrderingOrState(values, "MainPage");

            Assert.IsFalse(_brand.MainPage);
            _repo.Verify(r => r.Edit(_brand), Times.Once);
        }

        [TestMethod]
        public void ChangeGridBaseEntityOrderingOrState_WhenValuesNull_Throws()
        {
            try
            {
                _sut.ChangeGridBaseEntityOrderingOrState(null, "");
                Assert.Fail("Expected ArgumentException");
            }
            catch (System.ArgumentException)
            {
            }
        }
    }
}
