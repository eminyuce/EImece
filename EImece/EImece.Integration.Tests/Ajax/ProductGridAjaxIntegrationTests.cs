using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EImece.Areas.Admin.Controllers;
using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories;
using EImece.Domain.Services;
using EImece.Integration.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Integration.Tests.Ajax
{
    [TestClass]
    public class ProductGridAjaxIntegrationTests
    {
        [TestMethod]
        public void BrandService_ChangeOrdering_PersistsPosition()
        {
            LegacyTestDbFixture.RequireDb();
            using (var db = LegacyTestDbFixture.CreateContext())
            {
                var repo = new BrandRepository(db);
                var svc = new BrandService(repo);
                var brand = repo.GetSingle(LegacyTestDbFixture.SeededBrandId);
                Assert.IsNotNull(brand);

                svc.ChangeGridBaseEntityOrderingOrState(
                    new List<OrderingItem> { new OrderingItem { Id = brand.Id, Position = 42, IsActive = true } },
                    checkbox: "");

                var reloaded = repo.GetSingle(brand.Id);
                Assert.AreEqual(42, reloaded.Position);
            }
        }

        [TestMethod]
        public void BrandService_ChangeState_PersistsIsActive()
        {
            LegacyTestDbFixture.RequireDb();
            using (var db = LegacyTestDbFixture.CreateContext())
            {
                var repo = new BrandRepository(db);
                var svc = new BrandService(repo);
                var brand = repo.GetSingle(LegacyTestDbFixture.SeededBrandId);
                brand.IsActive = false;
                repo.Edit(brand);
                repo.Save();

                svc.ChangeGridBaseEntityOrderingOrState(
                    new List<OrderingItem> { new OrderingItem { Id = brand.Id, Position = brand.Position, IsActive = true } },
                    "State");

                Assert.IsTrue(repo.GetSingle(brand.Id).IsActive);
            }
        }

        [TestMethod]
        public void ProductService_ChangeProductState_PersistsState()
        {
            LegacyTestDbFixture.RequireDb();
            using (var db = LegacyTestDbFixture.CreateContext())
            {
                var repo = new ProductRepository(db);
                var svc = new ProductService(repo);
                svc.ChangeProductState(
                    new List<string> { LegacyTestDbFixture.SeededProductId.ToString() },
                    ProductState.LimitedStock);

                var product = repo.GetProduct(LegacyTestDbFixture.SeededProductId);
                Assert.AreEqual(ProductState.LimitedStock, product.StateEnum);
            }
        }

        [TestMethod]
        public void ProductService_DeleteBaseEntity_RemovesOrSoftDeletesProduct()
        {
            LegacyTestDbFixture.RequireDb();
            using (var db = LegacyTestDbFixture.CreateContext())
            {
                var categoryId = LegacyTestDbFixture.SeededCategoryId;
                var temp = new Product
                {
                    Name = "Temp Delete",
                    ProductCode = "TMP-DEL",
                    State = "NONE",
                    Price = 1,
                    IsActive = true,
                    ProductCategoryId = categoryId,
                    Lang = 1,
                    CreatedDate = System.DateTime.UtcNow,
                    UpdatedDate = System.DateTime.UtcNow
                };
                db.Products.Add(temp);
                db.SaveChanges();
                var id = temp.Id;

                var repo = new ProductRepository(db);
                var svc = new ProductService(repo);
                svc.DeleteBaseEntity(new List<string> { id.ToString() });

                var exists = db.Products.AsNoTracking().Any(p => p.Id == id);
                // Legacy delete may hard-delete or leave inactive — either is a side effect.
                var inactive = db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
                Assert.IsTrue(!exists || (inactive != null && !inactive.IsActive));
            }
        }

        [TestMethod]
        public async Task AdminAjax_ChangeBrandGridOrderingOrState_RoundTrip()
        {
            LegacyTestDbFixture.RequireDb();
            using (var db = LegacyTestDbFixture.CreateContext())
            {
                var brandService = new BrandService(new BrandRepository(db));
                var controller = new AjaxController(null) { BrandService = brandService };
                var values = new List<OrderingItem>
                {
                    new OrderingItem { Id = LegacyTestDbFixture.SeededBrandId, Position = 7, IsActive = true }
                };

                var result = await controller.ChangeBrandGridOrderingOrState(values, "");
                Assert.IsNotNull(result);

                var brand = new BrandRepository(db).GetSingle(LegacyTestDbFixture.SeededBrandId);
                Assert.AreEqual(7, brand.Position);
            }
        }
    }
}
