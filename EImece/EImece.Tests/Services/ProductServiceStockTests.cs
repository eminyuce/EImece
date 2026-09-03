using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class ProductServiceStockTests
    {
        private class ProductStockStore
        {
            public List<Product> Products { get; } = new List<Product>();
            public List<Product> Edited { get; } = new List<Product>();
            public int SaveCalls { get; private set; }

            public Product GetProduct(int id)
            {
                return Products.FirstOrDefault(p => p.Id == id);
            }

            public Task<Product> GetProductAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(GetProduct(id));
            }

            public void Edit(Product product)
            {
                Edited.Add(product);
            }

            public int Save()
            {
                SaveCalls++;
                return 1;
            }

            public Task<int> SaveAsync()
            {
                SaveCalls++;
                return Task.FromResult(1);
            }
        }

        private static ProductService CreateService(ProductStockStore store)
        {
            var productRepo = new FakeServiceProxy<IProductRepository>(store).Instance;
            var cache = new FakeServiceProxy<IEimeceCacheProvider>(new object()).Instance;
            var settingService = new FakeServiceProxy<ISettingService>(new object()).Instance;
            var fileStorage = new FakeServiceProxy<IFileStorageService>(new object()).Instance;
            var filesHelper = (FilesHelper)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(FilesHelper));
            return new ProductService(
                productRepo,
                cache,
                settingService,
                fileStorage,
                new NullCurrentUserContext(),
                filesHelper,
                new FakeServiceProxy<IProductCategoryService>(new object()).Instance,
                new FakeServiceProxy<IProductCommentRepository>(new object()).Instance,
                new FakeServiceProxy<IOrderProductRepository>(new object()).Instance,
                new FakeServiceProxy<ITagService>(new object()).Instance,
                new FakeServiceProxy<ITemplateService>(new object()).Instance,
                new FakeServiceProxy<IProductTagRepository>(new object()).Instance,
                new FakeServiceProxy<IProductSpecificationRepository>(new object()).Instance,
                new FakeServiceProxy<IEntityFactory>(new object()).Instance,
                new FakeServiceProxy<IMenuService>(new object()).Instance,
                new FakeServiceProxy<ITagCategoryService>(new object()).Instance,
                TestNullLoggers.Create<ProductService>());
        }

        [TestMethod]
        public void DecreaseStock_IgnoresNonPositiveProductIdOrQuantity()
        {
            var store = new ProductStockStore();
            store.Products.Add(new Product { Id = 5, Name = "Kept" });
            var service = CreateService(store);

            service.DecreaseStock(0, 2);
            service.DecreaseStock(-1, 2);
            service.DecreaseStock(5, 0);
            service.DecreaseStock(5, -3);

            Assert.AreEqual(0, store.Edited.Count);
            Assert.AreEqual(0, store.SaveCalls);
        }

        [TestMethod]
        public void DecreaseStock_DoesNotFailWhenProductIsMissing()
        {
            var store = new ProductStockStore();
            var service = CreateService(store);

            service.DecreaseStock(404, 1);

            Assert.AreEqual(0, store.Edited.Count);
            Assert.AreEqual(0, store.SaveCalls);
        }

        [TestMethod]
        public void DecreaseStock_TouchesExistingProductAndPersists()
        {
            var store = new ProductStockStore();
            var product = new Product { Id = 12, Name = "Camera", UpdatedDate = DateTime.Now.AddDays(-2) };
            store.Products.Add(product);
            var service = CreateService(store);

            service.DecreaseStock(12, 3);

            Assert.AreEqual(1, store.Edited.Count);
            Assert.AreEqual(1, store.SaveCalls);
            Assert.AreSame(product, store.Edited[0]);
            Assert.IsTrue(product.UpdatedDate > DateTime.Now.AddMinutes(-1));
        }

        [TestMethod]
        public async Task DecreaseStockAsync_IgnoresInvalidInputAndMissingProducts()
        {
            var store = new ProductStockStore();
            var service = CreateService(store);

            await service.DecreaseStockAsync(0, 1);
            await service.DecreaseStockAsync(9, 0);
            await service.DecreaseStockAsync(9, 2);

            Assert.AreEqual(0, store.Edited.Count);
            Assert.AreEqual(0, store.SaveCalls);
        }

        [TestMethod]
        public async Task DecreaseStockAsync_PersistsTouchForExistingProduct()
        {
            var store = new ProductStockStore();
            store.Products.Add(new Product { Id = 3, Name = "Lens" });
            var service = CreateService(store);

            await service.DecreaseStockAsync(3, 1);

            Assert.AreEqual(1, store.Edited.Count);
            Assert.AreEqual(1, store.SaveCalls);
        }
    }
}
