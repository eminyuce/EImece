using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Integration.Tests.Infrastructure
{
    /// <summary>
    /// LocalDB fixture for EImece_Legacy_Test. Never points at production yuva8905_yuvadan.
    /// </summary>
    public static class LegacyTestDatabase
    {
        public const string Catalog = "EImece_Legacy_Test";

        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["EImeceDbConnection"]?.ConnectionString
            ?? $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={Catalog};Integrated Security=True;MultipleActiveResultSets=True";

        public static bool TryEnsureCreated(out string error)
        {
            error = null;
            try
            {
                Database.SetInitializer(new CreateDatabaseIfNotExists<EImeceContext>());
                using (var db = new EImeceContext(ConnectionString))
                {
                    db.Database.Initialize(force: false);
                    if (!db.Database.Exists())
                    {
                        error = "LocalDB database was not created.";
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static void RequireLocalDb()
        {
            if (!TryEnsureCreated(out var error))
            {
                Assert.Inconclusive("LocalDB EImece_Legacy_Test unavailable: " + error);
            }
        }

        public static SeedBundle SeedMinimalCatalog()
        {
            RequireLocalDb();
            using (var db = new EImeceContext(ConnectionString))
            {
                var category = new ProductCategory
                {
                    Name = "IT-Category-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    IsActive = true,
                    Position = 1,
                    Lang = 1,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
                db.ProductCategories.Add(category);
                db.SaveChanges();

                var brand = new Brand
                {
                    Name = "IT-Brand-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    IsActive = true,
                    Position = 1,
                    Lang = 1,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
                db.Brands.Add(brand);
                db.SaveChanges();

                var product = new Product
                {
                    Name = "IT-Product-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    ProductCode = "IT-" + Guid.NewGuid().ToString("N").Substring(0, 6),
                    ProductCategoryId = category.Id,
                    BrandId = brand.Id,
                    Price = 100m,
                    State = "ProductInStock",
                    IsActive = true,
                    Position = 0,
                    MainPage = false,
                    IsCampaign = false,
                    Lang = 1,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
                db.Products.Add(product);

                var order = new Order
                {
                    Name = "IT-Order",
                    OrderNumber = "ITORD-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    OrderGuid = Guid.NewGuid().ToString(),
                    OrderStatus = 1,
                    IsActive = true,
                    Lang = 1,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    DeliveryDate = DateTime.UtcNow
                };
                db.Orders.Add(order);
                db.SaveChanges();

                return new SeedBundle
                {
                    ProductId = product.Id,
                    BrandId = brand.Id,
                    CategoryId = category.Id,
                    OrderId = order.Id
                };
            }
        }

        public static bool CanConnect()
        {
            try
            {
                using (var conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=5"))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public sealed class SeedBundle
        {
            public int ProductId { get; set; }
            public int BrandId { get; set; }
            public int CategoryId { get; set; }
            public int OrderId { get; set; }
        }
    }
}
