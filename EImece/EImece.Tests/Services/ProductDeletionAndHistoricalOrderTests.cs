using EImece.Domain.Caching;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class ProductDeletionAndHistoricalOrderTests
    {
        private class FakeRepositoryProxy<TInterface> : RealProxy where TInterface : class
        {
            private readonly object _target;

            public FakeRepositoryProxy(object target) : base(typeof(TInterface))
            {
                _target = target;
            }

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                try
                {
                    var methods = _target.GetType().GetMethods().Where(m => m.Name == call.MethodName && m.GetParameters().Length == call.Args.Length).ToList();
                    if (methods.Count > 0)
                    {
                        var result = methods[0].Invoke(_target, call.Args);
                        return new ReturnMessage(result, null, 0, call.LogicalCallContext, call);
                    }
                }
                catch (TargetInvocationException ex)
                {
                    return new ReturnMessage(ex.InnerException, call);
                }

                object defaultResult = null;
                if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType == typeof(Task))
                    {
                        defaultResult = Task.CompletedTask;
                    }
                    else if (mi.ReturnType.IsGenericType && mi.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        var innerType = mi.ReturnType.GetGenericArguments()[0];
                        var defaultInner = innerType.IsValueType ? Activator.CreateInstance(innerType) : null;
                        defaultResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(innerType).Invoke(null, new[] { defaultInner });
                    }
                    else if (mi.ReturnType.IsValueType)
                    {
                        defaultResult = Activator.CreateInstance(mi.ReturnType);
                    }
                }
                return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
            }

            public TInterface Instance => (TInterface)GetTransparentProxy();
        }

        private class OrderProductMockStore
        {
            public List<OrderProduct> Items { get; } = new List<OrderProduct>();

            public IQueryable<OrderProduct> FindBy(Expression<Func<OrderProduct, bool>> predicate)
            {
                return Items.AsQueryable().Where(predicate);
            }

            public int SaveOrEdit(OrderProduct entity)
            {
                if (entity.Id == 0) entity.Id = Items.Count + 1;
                var existing = Items.FirstOrDefault(x => x.Id == entity.Id);
                if (existing != null)
                {
                    Items.Remove(existing);
                }
                Items.Add(entity);
                return entity.Id;
            }

            public Task<int> SaveOrEditAsync(OrderProduct entity)
            {
                return Task.FromResult(SaveOrEdit(entity));
            }
        }

        private class ProductMockStore
        {
            public List<Product> Products { get; } = new List<Product>();

            public Product GetProduct(int id)
            {
                return Products.FirstOrDefault(p => p.Id == id);
            }

            public Task<Product> GetProductAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(GetProduct(id));
            }

            public int DeleteItem(Product entity)
            {
                Products.Remove(entity);
                return 1;
            }

            public Task<int> DeleteItemAsync(Product entity)
            {
                Products.Remove(entity);
                return Task.FromResult(1);
            }
        }

        private class FileStorageMockStore
        {
            public List<int> DeletedFileStorageIds { get; } = new List<int>();
            public List<int> DeletedGalleryProductIds { get; } = new List<int>();

            public string DeleteFileStorage(int id)
            {
                DeletedFileStorageIds.Add(id);
                return "deleted";
            }

            public Task<string> DeleteFileStorageAsync(int id)
            {
                DeletedFileStorageIds.Add(id);
                return Task.FromResult("deleted");
            }

            public void DeleteGalleryImages(int contentId, MediaModType mod)
            {
                DeletedGalleryProductIds.Add(contentId);
            }

            public Task DeleteGalleryImagesAsync(int contentId, MediaModType mod)
            {
                DeletedGalleryProductIds.Add(contentId);
                return Task.CompletedTask;
            }
        }

        [TestMethod]
        public void Scenario1_OrderProductCapturesCompleteHistoricalSnapshot()
        {
            // Arrange: Product with price $100
            var product = new Product
            {
                Id = 42,
                Name = "Historical Camera",
                Price = 100.00m,
                ProductCode = "CAM-001",
                ProductCategoryId = 5,
                ImageState = true
            };

            var specItems = new List<EImece.Domain.Models.FrontModels.ProductSpecItem>
            {
                new EImece.Domain.Models.FrontModels.ProductSpecItem { SpecsName = "Color", SpecsValue = "Black" }
            };

            var cartProduct = new ShoppingCartProduct(product, specItems);

            // Act: Build OrderProduct snapshot
            var orderProduct = new OrderProduct
            {
                OrderId = 1001,
                ProductId = cartProduct.Id,
                ProductName = cartProduct.Name,
                ProductCode = cartProduct.ProductCode,
                CategoryName = cartProduct.CategoryName,
                ProductSalePrice = cartProduct.Price,
                Quantity = 2,
                TotalPrice = cartProduct.Price * 2,
                ProductSpecItems = JsonConvert.SerializeObject(cartProduct.ProductSpecItems),
                ProductImageUrl = cartProduct.CroppedImageUrl
            };

            // Assert: Snapshot fields are immutable
            Assert.AreEqual(42, orderProduct.ProductId);
            Assert.AreEqual("Historical Camera", orderProduct.ProductName);
            Assert.AreEqual("CAM-001", orderProduct.ProductCode);
            Assert.AreEqual(100.00m, orderProduct.ProductSalePrice);
            Assert.AreEqual(2, orderProduct.Quantity);
            Assert.AreEqual(200.00m, orderProduct.TotalPrice);
            Assert.AreEqual(100.00m, orderProduct.Price);
            Assert.AreEqual(1, orderProduct.ProductSpecObjItems.Count);
            Assert.AreEqual("Color", orderProduct.ProductSpecObjItems[0].SpecsName);
            Assert.AreEqual("Black", orderProduct.ProductSpecObjItems[0].SpecsValue);
        }

        [TestMethod]
        public void Scenario2_ProductPriceMutationDoesNotAffectHistoricalOrder()
        {
            // Arrange: Historical order created when product was $100
            var product = new Product
            {
                Id = 10,
                Name = "Designer Jacket",
                Price = 100m,
                ProductCode = "JKT-01"
            };

            var orderProduct = new OrderProduct
            {
                Id = 1,
                OrderId = 50,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductCode = product.ProductCode,
                ProductSalePrice = 100m,
                Quantity = 1,
                TotalPrice = 100m
            };

            // Act: Administrator changes product price to $150
            product.Price = 150m;

            // Assert: Order item price and totals remain $100
            Assert.AreEqual(100m, orderProduct.ProductSalePrice);
            Assert.AreEqual(100m, orderProduct.TotalPrice);
            Assert.AreEqual(100m, orderProduct.Price);
            Assert.AreNotEqual(product.Price, orderProduct.ProductSalePrice);
        }

        [TestMethod]
        public void Scenario3_ProductDeletionAllowsDeletingProductAndUnlinksProductId()
        {
            // Arrange
            var productStore = new ProductMockStore();
            var orderProductStore = new OrderProductMockStore();

            var product = new Product
            {
                Id = 77,
                Name = "Vintage Watch",
                Price = 250m,
                ProductCode = "WTC-77"
            };
            productStore.Products.Add(product);

            var orderProduct = new OrderProduct
            {
                Id = 101,
                OrderId = 99,
                ProductId = 77,
                ProductName = "Vintage Watch",
                ProductCode = "WTC-77",
                CategoryName = "Watches",
                ProductSalePrice = 250m,
                Quantity = 1,
                TotalPrice = 250m
            };
            orderProductStore.Items.Add(orderProduct);

            var fakeProductRepo = new FakeRepositoryProxy<IProductRepository>(productStore).Instance;
            var fakeOrderProductRepo = new FakeRepositoryProxy<IOrderProductRepository>(orderProductStore).Instance;

            var productService = CreateProductService(fakeProductRepo, fakeOrderProductRepo);

            // Act: Delete product
            var result = productService.DeleteProductById(77);

            // Assert: Product is deleted successfully
            Assert.AreEqual(ProductDeleteResult.Deleted, result);
            Assert.IsNull(productStore.GetProduct(77));

            // OrderProduct record is preserved and ProductId is unlinked (null)
            var preservedOrderProduct = orderProductStore.Items.FirstOrDefault(op => op.Id == 101);
            Assert.IsNotNull(preservedOrderProduct);
            Assert.IsNull(preservedOrderProduct.ProductId);
            Assert.AreEqual("Vintage Watch", preservedOrderProduct.ProductName);
            Assert.AreEqual(250m, preservedOrderProduct.ProductSalePrice);
            Assert.AreEqual(250m, preservedOrderProduct.TotalPrice);
            Assert.AreEqual(250m, preservedOrderProduct.Price);
        }

        [TestMethod]
        public async Task Scenario4_ProductDeletionAsyncAllowsDeletingProductAndPreservesOrderAsync()
        {
            // Arrange
            var productStore = new ProductMockStore();
            var orderProductStore = new OrderProductMockStore();

            var product = new Product
            {
                Id = 88,
                Name = "Leather Belt",
                Price = 50m,
                ProductCode = "BLT-88"
            };
            productStore.Products.Add(product);

            var orderProduct = new OrderProduct
            {
                Id = 202,
                OrderId = 60,
                ProductId = 88,
                ProductName = "Leather Belt",
                ProductCode = "BLT-88",
                CategoryName = "Accessories",
                ProductSalePrice = 50m,
                Quantity = 2,
                TotalPrice = 100m
            };
            orderProductStore.Items.Add(orderProduct);

            var fakeProductRepo = new FakeRepositoryProxy<IProductRepository>(productStore).Instance;
            var fakeOrderProductRepo = new FakeRepositoryProxy<IOrderProductRepository>(orderProductStore).Instance;

            var productService = CreateProductService(fakeProductRepo, fakeOrderProductRepo);

            // Act: Delete product async
            var result = await productService.DeleteProductByIdAsync(88);

            // Assert: Product deleted, order preserved
            Assert.AreEqual(ProductDeleteResult.Deleted, result);
            Assert.IsNull(await productStore.GetProductAsync(88));

            var preservedOrderProduct = orderProductStore.Items.FirstOrDefault(op => op.Id == 202);
            Assert.IsNotNull(preservedOrderProduct);
            Assert.IsNull(preservedOrderProduct.ProductId);
            Assert.AreEqual("Leather Belt", preservedOrderProduct.ProductName);
            Assert.AreEqual(50m, preservedOrderProduct.ProductSalePrice);
            Assert.AreEqual(100m, preservedOrderProduct.TotalPrice);
            Assert.AreEqual(50m, preservedOrderProduct.Price);
        }

        [TestMethod]
        public void Scenario5_MultipleProductsInOrder_DeletingOneLeavesOrderIntact()
        {
            // Arrange: Order with Product A and Product B
            var productStore = new ProductMockStore();
            var orderProductStore = new OrderProductMockStore();

            var productA = new Product { Id = 1, Name = "Product A", Price = 30m, ProductCode = "A01" };
            var productB = new Product { Id = 2, Name = "Product B", Price = 70m, ProductCode = "B02" };
            productStore.Products.Add(productA);
            productStore.Products.Add(productB);

            var opA = new OrderProduct
            {
                Id = 1,
                OrderId = 700,
                ProductId = 1,
                ProductName = "Product A",
                ProductCode = "A01",
                ProductSalePrice = 30m,
                Quantity = 1,
                TotalPrice = 30m
            };

            var opB = new OrderProduct
            {
                Id = 2,
                OrderId = 700,
                ProductId = 2,
                ProductName = "Product B",
                ProductCode = "B02",
                ProductSalePrice = 70m,
                Quantity = 1,
                TotalPrice = 70m
            };

            orderProductStore.Items.Add(opA);
            orderProductStore.Items.Add(opB);

            var fakeProductRepo = new FakeRepositoryProxy<IProductRepository>(productStore).Instance;
            var fakeOrderProductRepo = new FakeRepositoryProxy<IOrderProductRepository>(orderProductStore).Instance;

            var productService = CreateProductService(fakeProductRepo, fakeOrderProductRepo);

            // Act: Delete Product A only
            var result = productService.DeleteProductById(1);

            // Assert
            Assert.AreEqual(ProductDeleteResult.Deleted, result);
            Assert.IsNull(productStore.GetProduct(1));
            Assert.IsNotNull(productStore.GetProduct(2));

            var itemA = orderProductStore.Items.FirstOrDefault(x => x.Id == 1);
            var itemB = orderProductStore.Items.FirstOrDefault(x => x.Id == 2);

            Assert.IsNotNull(itemA);
            Assert.IsNull(itemA.ProductId);
            Assert.AreEqual("Product A", itemA.ProductName);
            Assert.AreEqual(30m, itemA.TotalPrice);

            Assert.IsNotNull(itemB);
            Assert.AreEqual(2, itemB.ProductId);
            Assert.AreEqual("Product B", itemB.ProductName);
            Assert.AreEqual(70m, itemB.TotalPrice);

            // Total for the order remains $100
            var orderTotal = orderProductStore.Items.Where(x => x.OrderId == 700).Sum(x => x.TotalPrice);
            Assert.AreEqual(100m, orderTotal);
        }

        [TestMethod]
        public void Scenario6_NullProductNavigationDoesNotThrowAndRendersSnapshot()
        {
            // Arrange: OrderProduct where Product navigation property is null (e.g. deleted product)
            var op = new OrderProduct
            {
                Id = 5,
                OrderId = 300,
                ProductId = null,
                ProductName = "Archived Item",
                ProductCode = "ARC-99",
                CategoryName = "Archive",
                ProductSalePrice = 120m,
                Quantity = 2,
                TotalPrice = 240m,
                Product = null
            };

            // Act & Assert
            Assert.AreEqual("Archived Item", op.Name);
            Assert.AreEqual(2, op.Count);
            Assert.AreEqual(120m, op.Price);
            Assert.AreEqual(240m, op.TotalPrice);
            Assert.IsNull(op.Product);
        }

        [TestMethod]
        public void Scenario7_ProductWithOrderHistory_PreservesImageFilesOnDeletion()
        {
            // Arrange: Product with MainImageId = 55 and an existing order
            var productStore = new ProductMockStore();
            var orderProductStore = new OrderProductMockStore();
            var fileStorageStore = new FileStorageMockStore();

            var product = new Product
            {
                Id = 150,
                Name = "Ordered Canvas Shoes",
                Price = 60m,
                ProductCode = "SH-150",
                MainImageId = 55
            };
            productStore.Products.Add(product);

            var orderProduct = new OrderProduct
            {
                Id = 301,
                OrderId = 800,
                ProductId = 150,
                ProductName = "Ordered Canvas Shoes",
                ProductCode = "SH-150",
                ProductImageUrl = "/Media/upload/shoes_thumb.jpg",
                ProductSalePrice = 60m,
                Quantity = 1,
                TotalPrice = 60m
            };
            orderProductStore.Items.Add(orderProduct);

            var fakeProductRepo = new FakeRepositoryProxy<IProductRepository>(productStore).Instance;
            var fakeOrderProductRepo = new FakeRepositoryProxy<IOrderProductRepository>(orderProductStore).Instance;
            var fakeFileStorageService = new FakeRepositoryProxy<IFileStorageService>(fileStorageStore).Instance;

            var productService = CreateProductService(fakeProductRepo, fakeOrderProductRepo, fakeFileStorageService);

            // Act: Delete product with order history
            var result = productService.DeleteProductById(150);

            // Assert
            Assert.AreEqual(ProductDeleteResult.Deleted, result);
            Assert.IsNull(productStore.GetProduct(150));
            // FileStorageService must NOT have deleted the image or gallery because of order history
            Assert.AreEqual(0, fileStorageStore.DeletedFileStorageIds.Count, "Image file must not be deleted when product has order history.");
            Assert.AreEqual(0, fileStorageStore.DeletedGalleryProductIds.Count, "Gallery images must not be deleted when product has order history.");
        }

        [TestMethod]
        public void Scenario8_ProductWithoutOrderHistory_DeletesImageFilesOnDeletion()
        {
            // Arrange: Product with MainImageId = 77 and NO orders
            var productStore = new ProductMockStore();
            var orderProductStore = new OrderProductMockStore();
            var fileStorageStore = new FileStorageMockStore();

            var product = new Product
            {
                Id = 160,
                Name = "Unordered Prototype",
                Price = 99m,
                ProductCode = "PROTO-99",
                MainImageId = 77
            };
            productStore.Products.Add(product);

            var fakeProductRepo = new FakeRepositoryProxy<IProductRepository>(productStore).Instance;
            var fakeOrderProductRepo = new FakeRepositoryProxy<IOrderProductRepository>(orderProductStore).Instance;
            var fakeFileStorageService = new FakeRepositoryProxy<IFileStorageService>(fileStorageStore).Instance;

            var productService = CreateProductService(fakeProductRepo, fakeOrderProductRepo, fakeFileStorageService);

            // Act: Delete product with no order history
            var result = productService.DeleteProductById(160);

            // Assert
            Assert.AreEqual(ProductDeleteResult.Deleted, result);
            Assert.IsNull(productStore.GetProduct(160));
            // FileStorageService MUST delete image and gallery files for unused product
            CollectionAssert.Contains(fileStorageStore.DeletedFileStorageIds, 77, "Main image storage must be cleaned up when product has no order history.");
            CollectionAssert.Contains(fileStorageStore.DeletedGalleryProductIds, 160, "Gallery images must be cleaned up when product has no order history.");
        }

        private static ProductService CreateProductService(
            IProductRepository productRepo,
            IOrderProductRepository orderProductRepo = null,
            IFileStorageService fileStorageService = null)
        {
            var cache = new FakeRepositoryProxy<IEimeceCacheProvider>(new object()).Instance;
            var settingService = new FakeRepositoryProxy<ISettingService>(new object()).Instance;
            var currentUserContext = new EImece.Tests.Infrastructure.NullCurrentUserContext();
            var filesHelper = (FilesHelper)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(FilesHelper));
            var categoryServ = new FakeRepositoryProxy<IProductCategoryService>(new object()).Instance;
            var commentRepo = new FakeRepositoryProxy<IProductCommentRepository>(new object()).Instance;
            var tagServ = new FakeRepositoryProxy<ITagService>(new object()).Instance;
            var templateServ = new FakeRepositoryProxy<ITemplateService>(new object()).Instance;
            var productTagRepo = new FakeRepositoryProxy<IProductTagRepository>(new object()).Instance;
            var specRepo = new FakeRepositoryProxy<IProductSpecificationRepository>(new object()).Instance;
            var entityFactory = new FakeRepositoryProxy<IEntityFactory>(new object()).Instance;
            var menuService = new FakeRepositoryProxy<IMenuService>(new object()).Instance;
            var tagCategoryServ = new FakeRepositoryProxy<ITagCategoryService>(new object()).Instance;

            return new ProductService(
                productRepo,
                cache,
                settingService,
                fileStorageService ?? new FakeRepositoryProxy<IFileStorageService>(new object()).Instance,
                currentUserContext,
                filesHelper,
                categoryServ,
                commentRepo,
                orderProductRepo ?? new FakeRepositoryProxy<IOrderProductRepository>(new object()).Instance,
                tagServ,
                templateServ,
                productTagRepo,
                specRepo,
                entityFactory,
                menuService,
                tagCategoryServ);
        }
    }
}
