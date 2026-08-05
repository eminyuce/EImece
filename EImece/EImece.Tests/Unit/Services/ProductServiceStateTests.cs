using System.Collections.Generic;
using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EImece.Tests.Unit.Services
{
    [TestClass]
    public class ProductServiceStateTests
    {
        [TestMethod]
        public void ChangeProductState_UpdatesStateEnumAndSaves()
        {
            var product = new Product { Id = 7, Name = "P", ProductCode = "X", State = "NONE" };
            var repo = new Mock<IProductRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetProduct(7)).Returns(product);
            repo.Setup(r => r.Edit(product));
            repo.Setup(r => r.Save()).Returns(1);

            var sut = new ProductService(repo.Object);
            sut.ChangeProductState(new List<string> { "7" }, ProductState.LimitedStock);

            Assert.AreEqual(ProductState.LimitedStock, product.StateEnum);
            repo.Verify(r => r.Edit(product), Times.Once);
            repo.Verify(r => r.Save(), Times.Once);
        }

        [TestMethod]
        public void ChangeProductState_WhenValuesEmpty_DoesNothing()
        {
            var repo = new Mock<IProductRepository>(MockBehavior.Strict);
            var sut = new ProductService(repo.Object);
            sut.ChangeProductState(new List<string>(), ProductState.ProductInStock);
            repo.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void ChangeProductState_WhenValuesNull_DoesNothing()
        {
            var repo = new Mock<IProductRepository>(MockBehavior.Strict);
            var sut = new ProductService(repo.Object);
            sut.ChangeProductState(null, ProductState.ProductInStock);
            repo.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void ChangeGridBaseEntityOrderingOrState_WhenMainPage_UpdatesProductMainPage()
        {
            var product = new Product
            {
                Id = 3,
                Name = "P",
                ProductCode = "C",
                State = "ProductInStock",
                MainPage = false
            };
            var repo = new Mock<IProductRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetSingle(3)).Returns(product);
            repo.Setup(r => r.Edit(product));
            repo.Setup(r => r.Save()).Returns(1);

            var sut = new ProductService(repo.Object);
            sut.ChangeGridBaseEntityOrderingOrState(
                new List<OrderingItem> { new OrderingItem { Id = 3, IsActive = true } },
                "MainPage");

            Assert.IsTrue(product.MainPage);
        }

        [TestMethod]
        public void UpdatePrices_WhenPercentageMissing_ReturnsHata()
        {
            var sut = new ProductService(Mock.Of<IProductRepository>());
            var result = sut.UpdatePrices(new EImece.Domain.Models.AdminModels.UpdatePriceRequest());
            Assert.AreEqual("hata", result);
        }
    }
}
