using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EImece.Areas.Admin.Controllers;
using EImece.Domain.DbContext;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories;
using EImece.Domain.Services;
using EImece.Integration.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Integration.Tests.Ajax
{
    [TestClass]
    public class AdminAjaxProductGridTests
    {
        [TestMethod]
        public async Task ChangeProductGridOrderingOrState_PersistsPositionAndIsActive()
        {
            if (!LegacyTestDatabase.CanConnect())
            {
                Assert.Inconclusive("LocalDB not available");
            }

            var seed = LegacyTestDatabase.SeedMinimalCatalog();
            using (var db = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var productService = new ProductService(new ProductRepository(db));
                var controller = new AjaxController(null) { ProductService = productService };

                var values = new List<OrderingItem>
                {
                    new OrderingItem { Id = seed.ProductId, Position = 42, IsActive = true }
                };

                await controller.ChangeProductGridOrderingOrState(values, "");
                await controller.ChangeProductGridOrderingOrState(
                    new List<OrderingItem> { new OrderingItem { Id = seed.ProductId, IsActive = false } },
                    "State");
            }

            using (var verify = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var product = verify.Products.Single(p => p.Id == seed.ProductId);
                Assert.AreEqual(42, product.Position);
                Assert.IsFalse(product.IsActive);
            }
        }

        [TestMethod]
        public void ProductStateChanged_PersistsStateString()
        {
            if (!LegacyTestDatabase.CanConnect())
            {
                Assert.Inconclusive("LocalDB not available");
            }

            var seed = LegacyTestDatabase.SeedMinimalCatalog();
            using (var db = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var productService = new ProductService(new ProductRepository(db));
                var controller = new AjaxController(null) { ProductService = productService };
                controller.ProductStateChanged(
                    new List<string> { seed.ProductId.ToString() },
                    ((int)ProductState.LimitedStock).ToString());
            }

            using (var verify = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var product = verify.Products.Single(p => p.Id == seed.ProductId);
                Assert.AreEqual(nameof(ProductState.LimitedStock), product.State);
            }
        }

        [TestMethod]
        public void DeleteProductGridItem_SoftDeletesOrRemovesProduct()
        {
            if (!LegacyTestDatabase.CanConnect())
            {
                Assert.Inconclusive("LocalDB not available");
            }

            var seed = LegacyTestDatabase.SeedMinimalCatalog();
            using (var db = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var productService = new ProductService(new ProductRepository(db))
                {
                    // Avoid NRE when delete checks order history
                    OrderProductRepository = new OrderProductRepository(db)
                };
                var controller = new AjaxController(null) { ProductService = productService };
                var result = controller.DeleteProductGridItem(new List<string> { seed.ProductId.ToString() }) as JsonResult;
                Assert.IsNotNull(result);
            }

            using (var verify = new EImeceContext(LegacyTestDatabase.ConnectionString))
            {
                var stillThere = verify.Products.FirstOrDefault(p => p.Id == seed.ProductId);
                // Hard delete or soft — either is acceptable legacy behavior for unsold product
                Assert.IsTrue(stillThere == null || stillThere.IsActive == false);
            }
        }
    }
}
