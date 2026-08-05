using System;
using System.Data.Entity;
using System.Data.SqlClient;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Integration.Tests.Infrastructure
{
    /// <summary>
    /// Creates/opens LocalDB catalog EImece_Legacy_Test and seeds minimal rows.
    /// Tests skip when LocalDB is unavailable.
    /// </summary>
    [TestClass]
    public class LegacyTestDbFixture
    {
        public const string Catalog = "EImece_Legacy_Test";

        public static string ConnectionString =>
            $@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog={Catalog};Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=30";

        public static bool IsAvailable { get; private set; }

        public static int SeededProductId { get; private set; }
        public static int SeededBrandId { get; private set; }
        public static int SeededCategoryId { get; private set; }
        public static int SeededOrderId { get; private set; }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Environment.SetEnvironmentVariable(ConnectionStringProvider.EnvironmentVariableName, ConnectionString);
            try
            {
                EnsureLocalDb();
                Database.SetInitializer(new DropCreateDatabaseAlways<EImeceContext>());
                using (var db = new EImeceContext(ConnectionString))
                {
                    db.Database.Initialize(force: true);
                    Seed(db);
                }
                IsAvailable = true;
            }
            catch (Exception ex)
            {
                IsAvailable = false;
                context.WriteLine("LocalDB unavailable — integration tests will be skipped: " + ex.Message);
            }
        }

        public static void RequireDb()
        {
            if (!IsAvailable)
                Assert.Inconclusive("LocalDB EImece_Legacy_Test is not available on this machine.");
        }

        public static EImeceContext CreateContext() => new EImeceContext(ConnectionString);

        private static void EnsureLocalDb()
        {
            var master = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=15";
            using (var conn = new SqlConnection(master))
            {
                conn.Open();
            }
        }

        private static void Seed(EImeceContext db)
        {
            var category = new ProductCategory
            {
                Name = "IT Category",
                IsActive = true,
                Position = 1,
                Lang = 1,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            db.ProductCategories.Add(category);
            db.SaveChanges();
            SeededCategoryId = category.Id;

            var brand = new Brand
            {
                Name = "IT Brand",
                IsActive = true,
                MainPage = false,
                Position = 1,
                Lang = 1,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            db.Brands.Add(brand);
            db.SaveChanges();
            SeededBrandId = brand.Id;

            var product = new Product
            {
                Name = "IT Product",
                ProductCode = "IT-001",
                State = "ProductInStock",
                Price = 100m,
                IsActive = true,
                Position = 0,
                MainPage = false,
                IsCampaign = false,
                ProductCategoryId = category.Id,
                BrandId = brand.Id,
                Lang = 1,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            db.Products.Add(product);
            db.SaveChanges();
            SeededProductId = product.Id;

            var order = new Order
            {
                Name = "IT Order",
                OrderNumber = "IT-ORD-1",
                OrderGuid = Guid.NewGuid().ToString("N"),
                OrderStatus = 1,
                IsActive = true,
                Lang = 1,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                DeliveryDate = DateTime.UtcNow
            };
            db.Orders.Add(order);
            db.SaveChanges();
            SeededOrderId = order.Id;
        }
    }
}
