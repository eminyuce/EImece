using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EImece.Tests.Unit.Services
{
    [TestClass]
    public class OrderServiceTests
    {
        [TestMethod]
        public void GetByOrderNumber_AttachesCustomerWhenUserIdPresent()
        {
            var order = new Order { Id = 1, OrderNumber = "N1", UserId = "u1", Name = "o" };
            var customer = new Customer { Id = 2, UserId = "u1", Name = "C" };
            var orderRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
            orderRepo.Setup(r => r.GetByOrderNumber("N1")).Returns(order);
            var customerSvc = new Mock<ICustomerService>(MockBehavior.Strict);
            customerSvc.Setup(c => c.GetUserId("u1")).Returns(customer);

            var sut = new OrderService(orderRepo.Object, customerSvc.Object, Mock.Of<IOrderProductService>());
            var result = sut.GetByOrderNumber("N1");

            Assert.AreSame(customer, result.Customer);
        }

        [TestMethod]
        public void GetOrderById_AttachesCustomer()
        {
            var order = new Order { Id = 3, UserId = "u2", Name = "o", OrderNumber = "N" };
            var customer = new Customer { Id = 9, UserId = "u2", Name = "Cust" };
            var orderRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
            orderRepo.Setup(r => r.GetOrderById(3)).Returns(order);
            var customerSvc = new Mock<ICustomerService>(MockBehavior.Strict);
            customerSvc.Setup(c => c.GetUserId("u2")).Returns(customer);

            var sut = new OrderService(orderRepo.Object, customerSvc.Object, Mock.Of<IOrderProductService>());
            var result = sut.GetOrderById(3);
            Assert.AreSame(customer, result.Customer);
        }

        [TestMethod]
        public void SaveOrEditEntity_SetsUpdatedDate()
        {
            var order = new Order { Id = 10, Name = "o", OrderNumber = "X" };
            var orderRepo = new Mock<IOrderRepository>();
            orderRepo.Setup(r => r.SaveOrEdit(order)).Returns(1);

            var sut = new OrderService(orderRepo.Object, Mock.Of<ICustomerService>(), Mock.Of<IOrderProductService>());
            sut.SaveOrEditEntity(order);

            Assert.IsTrue(order.UpdatedDate > System.DateTime.MinValue);
            orderRepo.Verify(r => r.SaveOrEdit(order), Times.Once);
        }
    }
}
