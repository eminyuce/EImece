using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using EImece.Areas.Admin.Controllers;
using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Services.IServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace EImece.Tests.Unit.Controllers
{
    [TestClass]
    public class AdminAjaxControllerP0Tests
    {
        private Mock<IProductService> _products;
        private Mock<IOrderService> _orders;
        private Mock<IFileStorageService> _files;
        private Mock<IBrandService> _brands;
        private AjaxController _sut;

        [TestInitialize]
        public void Init()
        {
            _products = new Mock<IProductService>(MockBehavior.Strict);
            _orders = new Mock<IOrderService>(MockBehavior.Strict);
            _files = new Mock<IFileStorageService>(MockBehavior.Strict);
            _brands = new Mock<IBrandService>(MockBehavior.Loose);

            _sut = new AjaxController(null)
            {
                ProductService = _products.Object,
                OrderService = _orders.Object,
                FileStorageService = _files.Object,
                BrandService = _brands.Object
            };
        }

        [TestMethod]
        public void DeleteProductGridItem_CallsDeleteBaseEntity_AndReturnsValues()
        {
            var values = new List<string> { "10", "11" };
            _products.Setup(p => p.DeleteBaseEntity(values));

            var result = _sut.DeleteProductGridItem(values) as JsonResult;
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(values, (List<string>)result.Data);
            _products.Verify(p => p.DeleteBaseEntity(values), Times.Once);
        }

        [TestMethod]
        public async Task ChangeProductGridOrderingOrState_CallsService_AndReturnsPayload()
        {
            var values = new List<OrderingItem>
            {
                new OrderingItem { Id = 1, Position = 3, IsActive = true }
            };
            _products.Setup(p => p.ChangeGridBaseEntityOrderingOrStateAsync(values, "State"))
                .Returns(Task.CompletedTask);

            var result = await _sut.ChangeProductGridOrderingOrState(values, "State") as JsonResult;
            Assert.IsNotNull(result);
            var json = JsonConvert.SerializeObject(result.Data);
            StringAssert.Contains(json, "State");
            StringAssert.Contains(json, "\"Id\":1");
        }

        [TestMethod]
        public void ProductStateChanged_ParsesEnum_AndCallsChangeProductState()
        {
            var values = new List<string> { "5" };
            _products.Setup(p => p.ChangeProductState(values, ProductState.LimitedStock));

            var result = _sut.ProductStateChanged(values, ((int)ProductState.LimitedStock).ToString()) as JsonResult;
            Assert.IsNotNull(result);
            _products.Verify(p => p.ChangeProductState(values, ProductState.LimitedStock), Times.Once);
        }

        [TestMethod]
        public void UpdatePrices_WhenSuccess_ReturnsAffectedRows()
        {
            var request = new UpdatePriceRequest { PercentageOfIncreaseOrDecrease = 5m, BrandId = 2 };
            _products.Setup(p => p.UpdatePrices(request)).Returns("3");

            var result = _sut.UpdatePrices(request) as JsonResult;
            Assert.IsNotNull(result);
            var json = JsonConvert.SerializeObject(result.Data);
            StringAssert.Contains(json, "\"success\":true");
            StringAssert.Contains(json, "\"affectedRows\":\"3\"");
        }

        [TestMethod]
        public async Task SaveAdminOrderNote_UpdatesOrderFields()
        {
            var order = new Order { Id = 9, Name = "o", OrderNumber = "N" };
            _orders.Setup(o => o.GetSingleAsync(9)).ReturnsAsync(order);
            _orders.Setup(o => o.SaveOrEditEntityAsync(order)).ReturnsAsync(order);

            var result = await _sut.SaveAdminOrderNote(9, "note", "Yurtici", "TRK1") as JsonResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("note", order.AdminOrderNote);
            Assert.AreEqual("Yurtici", order.ShipmentCompanyName);
            Assert.AreEqual("TRK1", order.ShipmentTrackingNumber);
        }

        [TestMethod]
        public async Task ChangedOrderStatus_UpdatesOrderStatus()
        {
            var order = new Order { Id = 4, Name = "o", OrderNumber = "N", OrderStatus = 0 };
            _orders.Setup(o => o.GetSingleAsync(4)).ReturnsAsync(order);
            _orders.Setup(o => o.SaveOrEditEntityAsync(order)).ReturnsAsync(order);

            var result = await _sut.ChangedOrderStatus(4, "NewlyOrder") as JsonResult;
            Assert.IsNotNull(result);
            Assert.AreEqual((int)EImeceOrderStatus.NewlyOrder, order.OrderStatus);
        }

        [TestMethod]
        public void DeleteBaseContentMainImage_ForProduct_ClearsMainImage()
        {
            var product = new Product
            {
                Id = 2,
                Name = "P",
                ProductCode = "C",
                State = "NONE",
                MainImageId = 88
            };
            _files.Setup(f => f.DeleteFileStorage(88)).Returns("ok");
            _products.Setup(p => p.GetSingle(2)).Returns(product);
            _products.Setup(p => p.SaveOrEditEntity(product)).Returns(product);

            var result = _sut.DeleteBaseContentMainImage(2, 88, "Product") as JsonResult;
            Assert.IsNotNull(result);
            Assert.IsNull(product.MainImageId);
        }
    }
}
