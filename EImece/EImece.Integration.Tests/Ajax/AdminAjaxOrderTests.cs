using System.Linq;
using System.Threading.Tasks;
using EImece.Areas.Admin.Controllers;
using EImece.Domain.DbContext;
using EImece.Domain.Models.Enums;
using EImece.Domain.Repositories;
using EImece.Domain.Services;
using EImece.Integration.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EImece.Integration.Tests.Ajax
{
    [TestClass]
    public class AdminAjaxOrderTests
    {
        [TestMethod]
        public async Task SaveAdminOrderNote_And_ChangedOrderStatus_RoundTrip()
        {
            if (!LegacyTestDatabase.CanConnect())
            {
                Assert.Inconclusive("LocalDB not available");
            }

            var seed = LegacyTestDatabase.SeedMinimalCatalog();
            using (var db = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var orderService = new OrderService(
                    new OrderRepository(db),
                    Mock.Of<EImece.Domain.Services.IServices.ICustomerService>(),
                    Mock.Of<EImece.Domain.Services.IServices.IOrderProductService>());

                var controller = new AjaxController(null) { OrderService = orderService };
                await controller.SaveAdminOrderNote(seed.OrderId, "IT note", "Aras", "TRACK-1");
                await controller.ChangedOrderStatus(seed.OrderId, nameof(EImeceOrderStatus.Shipped));
            }

            using (var verify = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var order = verify.Orders.Single(o => o.Id == seed.OrderId);
                Assert.AreEqual("IT note", order.AdminOrderNote);
                Assert.AreEqual("Aras", order.ShipmentCompanyName);
                Assert.AreEqual("TRACK-1", order.ShipmentTrackingNumber);
                Assert.AreEqual((int)EImeceOrderStatus.Shipped, order.OrderStatus);
            }
        }
    }
}
