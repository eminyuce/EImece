using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EImece.Tests.Unit.Services
{
    [TestClass]
    public class ShoppingCartServiceTests
    {
        [TestMethod]
        public void GetShoppingCartByOrderGuid_ReturnsRepositoryResult()
        {
            var cart = new ShoppingCart { Id = 3, OrderGuid = "guid-1", Name = "cart" };
            var repo = new Mock<IShoppingCartRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetShoppingCartByOrderGuid("guid-1")).Returns(cart);

            var sut = new ShoppingCartService(
                userManager: null,
                repository: repo.Object,
                orderService: Mock.Of<IOrderService>(),
                customerService: Mock.Of<ICustomerService>(),
                addressService: Mock.Of<IAddressService>(),
                orderProductService: Mock.Of<IOrderProductService>());

            var result = sut.GetShoppingCartByOrderGuid("guid-1");
            Assert.AreSame(cart, result);
        }

        [TestMethod]
        public void SaveOrEditShoppingCart_WhenNew_SavesItem()
        {
            var item = new ShoppingCart { OrderGuid = "new-guid", ShoppingCartJson = "{}", Name = "c" };
            var repo = new Mock<IShoppingCartRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetShoppingCartByOrderGuid("new-guid")).Returns((ShoppingCart)null);
            repo.Setup(r => r.SaveOrEdit(item)).Returns(1);

            var sut = new ShoppingCartService(
                null, repo.Object,
                Mock.Of<IOrderService>(), Mock.Of<ICustomerService>(),
                Mock.Of<IAddressService>(), Mock.Of<IOrderProductService>());

            sut.SaveOrEditShoppingCart(item);
            repo.Verify(r => r.SaveOrEdit(item), Times.Once);
        }

        [TestMethod]
        public void SaveOrEditShoppingCart_WhenExists_UpdatesJson()
        {
            var existing = new ShoppingCart { Id = 9, OrderGuid = "g", ShoppingCartJson = "old", Name = "c" };
            var incoming = new ShoppingCart { OrderGuid = "g", ShoppingCartJson = "new", Name = "c" };
            var repo = new Mock<IShoppingCartRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetShoppingCartByOrderGuid("g")).Returns(existing);
            repo.Setup(r => r.SaveOrEdit(It.Is<ShoppingCart>(c => c.ShoppingCartJson == "new"))).Returns(1);

            var sut = new ShoppingCartService(
                null, repo.Object,
                Mock.Of<IOrderService>(), Mock.Of<ICustomerService>(),
                Mock.Of<IAddressService>(), Mock.Of<IOrderProductService>());

            sut.SaveOrEditShoppingCart(incoming);
            Assert.AreEqual("new", existing.ShoppingCartJson);
        }

    }
}
