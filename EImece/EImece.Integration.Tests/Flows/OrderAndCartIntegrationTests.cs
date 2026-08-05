using System;
using System.Threading.Tasks;
using EImece.Areas.Admin.Controllers;
using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Repositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Integration.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EImece.Integration.Tests.Flows
{
    [TestClass]
    public class OrderAndCartIntegrationTests
    {
        [TestMethod]
        public async Task SaveAdminOrderNote_PersistsNoteAndShipment()
        {
            LegacyTestDbFixture.RequireDb();
            using (var db = LegacyTestDbFixture.CreateContext())
            {
                var orderService = new OrderService(
                    new OrderRepository(db),
                    Mock.Of<ICustomerService>(),
                    Mock.Of<IOrderProductService>());
                var controller = new AjaxController(null) { OrderService = orderService };

                await controller.SaveAdminOrderNote(
                    LegacyTestDbFixture.SeededOrderId,
                    "IT note",
                    "MNG",
                    "TRACK-99");

                var order = new OrderRepository(db).GetSingle(LegacyTestDbFixture.SeededOrderId);
                Assert.AreEqual("IT note", order.AdminOrderNote);
                Assert.AreEqual("MNG", order.ShipmentCompanyName);
                Assert.AreEqual("TRACK-99", order.ShipmentTrackingNumber);
            }
        }

        [TestMethod]
        public async Task ChangedOrderStatus_PersistsStatus()
        {
            LegacyTestDbFixture.RequireDb();
            using (var db = LegacyTestDbFixture.CreateContext())
            {
                var orderService = new OrderService(
                    new OrderRepository(db),
                    Mock.Of<ICustomerService>(),
                    Mock.Of<IOrderProductService>());
                var controller = new AjaxController(null) { OrderService = orderService };

                await controller.ChangedOrderStatus(LegacyTestDbFixture.SeededOrderId, "NewlyOrder");

                var order = new OrderRepository(db).GetSingle(LegacyTestDbFixture.SeededOrderId);
                Assert.AreEqual(1, order.OrderStatus);
            }
        }

        [TestMethod]
        public void ShoppingCartService_SaveAndLoadByOrderGuid_RoundTrip()
        {
            LegacyTestDbFixture.RequireDb();
            using (var db = LegacyTestDbFixture.CreateContext())
            {
                var guid = Guid.NewGuid().ToString("N");
                var repo = new ShoppingCartRepository(db);
                var svc = new ShoppingCartService(
                    null, repo,
                    Mock.Of<IOrderService>(),
                    Mock.Of<ICustomerService>(),
                    Mock.Of<IAddressService>(),
                    Mock.Of<IOrderProductService>());

                svc.SaveOrEditShoppingCart(new ShoppingCart
                {
                    Name = "cart",
                    OrderGuid = guid,
                    ShoppingCartJson = "{\"items\":1}",
                    IsActive = true,
                    Lang = 1,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });

                var loaded = svc.GetShoppingCartByOrderGuid(guid);
                Assert.IsNotNull(loaded);
                Assert.AreEqual("{\"items\":1}", loaded.ShoppingCartJson);
            }
        }

        [TestMethod]
        public void ShoppingCartSession_WithCoupon_ComputesDiscountedTotal()
        {
            var session = new ShoppingCartSession();
            session.ShoppingCartItems.Add(new ShoppingCartItem
            {
                Quantity = 2,
                Product = new ShoppingCartProduct { Id = 1, Price = 50m, Name = "P" }
            });
            session.Coupon = new Coupon { Discount = 10, DiscountPercentage = 0 };

            Assert.AreEqual(100m, session.TotalPrice);
            Assert.AreEqual(10m, session.CalculateCouponDiscount(session.TotalPrice));
            Assert.AreEqual(90m, session.TotalPriceWithCargoPrice);
        }
    }
}
