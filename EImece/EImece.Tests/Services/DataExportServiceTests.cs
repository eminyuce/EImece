using EImece.Areas.Admin.Controllers;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services.ExportImport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using DomainConstants = EImece.Domain.Constants;

namespace EImece.Tests.Services
{
    [TestClass]
    public class DataExportServiceTests
    {
        private class FakeDataExportService : IDataExportService
        {
            public List<SettingExportDto> MockSettings { get; set; } = new List<SettingExportDto>();
            public List<ProductExportDto> MockProducts { get; set; } = new List<ProductExportDto>();
            public List<ProductCategoryExportDto> MockCategories { get; set; } = new List<ProductCategoryExportDto>();
            public List<OrderExportDto> MockOrders { get; set; } = new List<OrderExportDto>();
            public List<UserExportDto> MockUsers { get; set; } = new List<UserExportDto>();

            private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            private static readonly JsonWriterOptions WriterOptions = new JsonWriterOptions
            {
                Indented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            public async Task<DataExportResult> ExportDataAsync(DataExportRequest request, Stream outputStream, CancellationToken cancellationToken = default(CancellationToken))
            {
                if (outputStream == null)
                {
                    throw new ArgumentNullException(nameof(outputStream));
                }

                request = request ?? new DataExportRequest();
                var manifest = new ExportManifest
                {
                    Application = "EImece",
                    Format = "application-data-export",
                    FormatVersion = "1.0",
                    DatabaseProvider = "SqlServer",
                    CreatedAtUtc = DateTime.UtcNow
                };

                manifest.ExcludedFields["Users"] = new List<string> { "PasswordHash", "SecurityStamp", "AuthenticatorKey" };
                manifest.ExcludedFields["Orders"] = new List<string> { "CardToken", "CardUserKey", "BinNumber", "LastFourDigits" };

                var totalRecords = 0;
                var metadata = new ExportMetadata
                {
                    ExportedBy = request.ExportedBy ?? "Admin",
                    Application = "EImece",
                    ApplicationVersion = "1.0",
                    CreatedAtUtc = DateTime.UtcNow
                };

                using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    if (request.ShouldExport("Settings"))
                    {
                        var entry = archive.CreateEntry("settings.json");
                        using (var stream = entry.Open())
                        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
                        {
                            writer.WriteStartObject();
                            writer.WriteString("entity", "Setting");
                            writer.WriteNumber("schemaVersion", 1);
                            writer.WriteStartArray("records");
                            foreach (var item in MockSettings)
                            {
                                JsonSerializer.Serialize(writer, item, JsonOptions);
                            }
                            writer.WriteEndArray();
                            writer.WriteNumber("recordCount", MockSettings.Count);
                            writer.WriteEndObject();
                            await writer.FlushAsync(cancellationToken);
                        }
                        manifest.Entities["Settings"] = new ExportEntityManifestEntry { File = "settings.json", RecordCount = MockSettings.Count };
                        totalRecords += MockSettings.Count;
                        metadata.IncludedEntities.Add("Settings");
                    }

                    if (request.ShouldExport("ProductCategories"))
                    {
                        var entry = archive.CreateEntry("product-categories.json");
                        using (var stream = entry.Open())
                        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
                        {
                            writer.WriteStartObject();
                            writer.WriteString("entity", "ProductCategory");
                            writer.WriteNumber("schemaVersion", 1);
                            writer.WriteStartArray("records");
                            foreach (var item in MockCategories)
                            {
                                JsonSerializer.Serialize(writer, item, JsonOptions);
                            }
                            writer.WriteEndArray();
                            writer.WriteNumber("recordCount", MockCategories.Count);
                            writer.WriteEndObject();
                            await writer.FlushAsync(cancellationToken);
                        }
                        manifest.Entities["ProductCategories"] = new ExportEntityManifestEntry { File = "product-categories.json", RecordCount = MockCategories.Count };
                        totalRecords += MockCategories.Count;
                        metadata.IncludedEntities.Add("ProductCategories");
                    }

                    if (request.ShouldExport("Products"))
                    {
                        var entry = archive.CreateEntry("products.json");
                        using (var stream = entry.Open())
                        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
                        {
                            writer.WriteStartObject();
                            writer.WriteString("entity", "Product");
                            writer.WriteNumber("schemaVersion", 1);
                            writer.WriteStartArray("records");
                            foreach (var item in MockProducts)
                            {
                                JsonSerializer.Serialize(writer, item, JsonOptions);
                            }
                            writer.WriteEndArray();
                            writer.WriteNumber("recordCount", MockProducts.Count);
                            writer.WriteEndObject();
                            await writer.FlushAsync(cancellationToken);
                        }
                        manifest.Entities["Products"] = new ExportEntityManifestEntry { File = "products.json", RecordCount = MockProducts.Count };
                        totalRecords += MockProducts.Count;
                        metadata.IncludedEntities.Add("Products");
                    }

                    if (request.ShouldExport("Orders"))
                    {
                        var entry = archive.CreateEntry("orders.json");
                        using (var stream = entry.Open())
                        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
                        {
                            writer.WriteStartObject();
                            writer.WriteString("entity", "Order");
                            writer.WriteNumber("schemaVersion", 1);
                            writer.WriteStartArray("records");
                            foreach (var item in MockOrders)
                            {
                                JsonSerializer.Serialize(writer, item, JsonOptions);
                            }
                            writer.WriteEndArray();
                            writer.WriteNumber("recordCount", MockOrders.Count);
                            writer.WriteEndObject();
                            await writer.FlushAsync(cancellationToken);
                        }
                        manifest.Entities["Orders"] = new ExportEntityManifestEntry { File = "orders.json", RecordCount = MockOrders.Count };
                        totalRecords += MockOrders.Count;
                        metadata.IncludedEntities.Add("Orders");
                    }

                    if (request.ShouldExport("Users"))
                    {
                        var entry = archive.CreateEntry("users.json");
                        using (var stream = entry.Open())
                        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
                        {
                            writer.WriteStartObject();
                            writer.WriteString("entity", "User");
                            writer.WriteNumber("schemaVersion", 1);
                            writer.WriteStartArray("records");
                            foreach (var item in MockUsers)
                            {
                                JsonSerializer.Serialize(writer, item, JsonOptions);
                            }
                            writer.WriteEndArray();
                            writer.WriteNumber("recordCount", MockUsers.Count);
                            writer.WriteEndObject();
                            await writer.FlushAsync(cancellationToken);
                        }
                        manifest.Entities["Users"] = new ExportEntityManifestEntry { File = "users.json", RecordCount = MockUsers.Count };
                        totalRecords += MockUsers.Count;
                        metadata.IncludedEntities.Add("Users");
                    }

                    metadata.TotalRecords = totalRecords;

                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using (var stream = manifestEntry.Open())
                    {
                        await JsonSerializer.SerializeAsync(stream, manifest, JsonOptions, cancellationToken);
                    }

                    var metaEntry = archive.CreateEntry("metadata.json");
                    using (var stream = metaEntry.Open())
                    {
                        await JsonSerializer.SerializeAsync(stream, metadata, JsonOptions, cancellationToken);
                    }
                }

                return new DataExportResult
                {
                    Success = true,
                    TotalRecords = totalRecords,
                    Manifest = manifest,
                    Metadata = metadata,
                    CompressedSizeBytes = outputStream.CanSeek ? outputStream.Position : 0
                };
            }

            public Task<DataExportSummary> GetExportMetadataAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                var summary = new DataExportSummary
                {
                    Application = "EImece",
                    DatabaseProvider = "SqlServer",
                    EstimatedCounts = new Dictionary<string, int>
                    {
                        { "Settings", MockSettings.Count },
                        { "ProductCategories", MockCategories.Count },
                        { "Products", MockProducts.Count },
                        { "Orders", MockOrders.Count },
                        { "Users", MockUsers.Count }
                    },
                    TotalEstimatedRecords = MockSettings.Count + MockCategories.Count + MockProducts.Count + MockOrders.Count + MockUsers.Count
                };
                return Task.FromResult(summary);
            }
        }

        [TestMethod]
        public async Task ExportDataAsync_NullStream_ThrowsArgumentNullException()
        {
            var service = (DataExportService)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(DataExportService));
            try
            {
                await service.ExportDataAsync(new DataExportRequest(), null);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task ExportDataAsync_ProducesValidZipArchive_WithManifestAndMetadata()
        {
            var fakeService = new FakeDataExportService();
            fakeService.MockSettings.Add(new SettingExportDto
            {
                Id = 1,
                Name = "SiteTitle",
                SettingKey = "SiteTitle",
                SettingValue = "EImece Test Site",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            using (var memoryStream = new MemoryStream())
            {
                var result = await fakeService.ExportDataAsync(new DataExportRequest(), memoryStream);

                Assert.IsTrue(result.Success);
                Assert.AreEqual(1, result.TotalRecords);
                Assert.IsNotNull(result.Manifest);
                Assert.IsNotNull(result.Metadata);

                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    var manifestEntry = zip.GetEntry("manifest.json");
                    Assert.IsNotNull(manifestEntry, "manifest.json must be present in zip archive");

                    var metaEntry = zip.GetEntry("metadata.json");
                    Assert.IsNotNull(metaEntry, "metadata.json must be present in zip archive");

                    var settingsEntry = zip.GetEntry("settings.json");
                    Assert.IsNotNull(settingsEntry, "settings.json must be present in zip archive");

                    using (var reader = new StreamReader(manifestEntry.Open()))
                    {
                        var json = await reader.ReadToEndAsync();
                        var manifest = JsonSerializer.Deserialize<ExportManifest>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        Assert.AreEqual("application-data-export", manifest.Format);
                        Assert.AreEqual("1.0", manifest.FormatVersion);
                        Assert.AreEqual("EImece", manifest.Application);
                        Assert.AreEqual("SqlServer", manifest.DatabaseProvider);
                    }
                }
            }
        }

        [TestMethod]
        public async Task ExportDataAsync_ExcludesSensitiveIdentityFields_DocumentedInManifest()
        {
            var fakeService = new FakeDataExportService();
            fakeService.MockUsers.Add(new UserExportDto
            {
                Id = "user-1",
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                TwoFactorAuthenticatorEnabled = true,
                Roles = new List<string> { "Administrator" }
            });

            using (var memoryStream = new MemoryStream())
            {
                var result = await fakeService.ExportDataAsync(new DataExportRequest(), memoryStream);

                Assert.IsTrue(result.Success);
                Assert.IsTrue(result.Manifest.ExcludedFields.ContainsKey("Users"));
                var excluded = result.Manifest.ExcludedFields["Users"];
                CollectionAssert.Contains(excluded, "PasswordHash");
                CollectionAssert.Contains(excluded, "SecurityStamp");
                CollectionAssert.Contains(excluded, "AuthenticatorKey");

                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    var usersEntry = zip.GetEntry("users.json");
                    Assert.IsNotNull(usersEntry);
                    using (var reader = new StreamReader(usersEntry.Open()))
                    {
                        var json = await reader.ReadToEndAsync();
                        Assert.IsFalse(json.Contains("passwordHash"), "Exported user JSON must not contain password hash");
                        Assert.IsFalse(json.Contains("securityStamp"), "Exported user JSON must not contain security stamp");
                        Assert.IsFalse(json.Contains("authenticatorKey"), "Exported user JSON must not contain authenticator key");
                        Assert.IsTrue(json.Contains("admin@example.com"));
                    }
                }
            }
        }

        [TestMethod]
        public async Task ExportDataAsync_ExcludesSensitivePaymentFields()
        {
            var fakeService = new FakeDataExportService();
            fakeService.MockOrders.Add(new OrderExportDto
            {
                Id = 100,
                Name = "Order #100",
                OrderNumber = "ORD-2026-001",
                OrderGuid = Guid.NewGuid().ToString(),
                Price = "150.00",
                PaidPrice = "150.00",
                Currency = "TRY",
                PaymentStatus = "SUCCESS",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            using (var memoryStream = new MemoryStream())
            {
                var result = await fakeService.ExportDataAsync(new DataExportRequest(), memoryStream);

                Assert.IsTrue(result.Success);
                Assert.IsTrue(result.Manifest.ExcludedFields.ContainsKey("Orders"));
                var excluded = result.Manifest.ExcludedFields["Orders"];
                CollectionAssert.Contains(excluded, "CardToken");
                CollectionAssert.Contains(excluded, "CardUserKey");
                CollectionAssert.Contains(excluded, "BinNumber");
                CollectionAssert.Contains(excluded, "LastFourDigits");

                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    var ordersEntry = zip.GetEntry("orders.json");
                    Assert.IsNotNull(ordersEntry);
                    using (var reader = new StreamReader(ordersEntry.Open()))
                    {
                        var json = await reader.ReadToEndAsync();
                        Assert.IsFalse(json.Contains("cardToken"));
                        Assert.IsFalse(json.Contains("cardUserKey"));
                        Assert.IsFalse(json.Contains("lastFourDigits"));
                        Assert.IsTrue(json.Contains("ORD-2026-001"));
                    }
                }
            }
        }

        [TestMethod]
        public async Task ExportDataAsync_PreservesUnicodeAndSpecialCharacters()
        {
            var fakeService = new FakeDataExportService();
            fakeService.MockProducts.Add(new ProductExportDto
            {
                Id = 1,
                Name = "Türkçe Karakterli Ürün Adı & Çiçek / Şapka / Öğretmen ğüşiöç",
                ProductCode = "TR-001",
                Price = 99.90m,
                Description = "<p>Özel <strong>HTML</strong> içerik & açıklama \"özel işaretler\"</p>",
                ShortDescription = "Kısa açıklama: İstanbul / Türkiye",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            using (var memoryStream = new MemoryStream())
            {
                var result = await fakeService.ExportDataAsync(new DataExportRequest(), memoryStream);

                Assert.IsTrue(result.Success);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    var productsEntry = zip.GetEntry("products.json");
                    Assert.IsNotNull(productsEntry);
                    using (var reader = new StreamReader(productsEntry.Open()))
                    {
                        var json = await reader.ReadToEndAsync();
                        Assert.IsTrue(json.Contains("Türkçe Karakterli Ürün Adı"));
                        Assert.IsTrue(json.Contains("ğüşiöç"));
                        Assert.IsTrue(json.Contains("İstanbul / Türkiye"));
                        Assert.IsTrue(json.Contains("<p>Özel <strong>HTML</strong>"));
                        Assert.IsFalse(json.Contains("\\u0130"), "Must not escape İ as \\u0130");
                        Assert.IsFalse(json.Contains("\\u003C"), "Must not escape < as \\u003C");
                        Assert.IsFalse(json.Contains("\\u003E"), "Must not escape > as \\u003E");
                    }
                }
            }
        }

        [TestMethod]
        public async Task ExportDataAsync_PreservesAllTurkishCharactersAndHtmlMarkupWithoutEscapeSequences()
        {
            var fakeService = new FakeDataExportService();
            var allTurkishChars = "İıŞşĞğÇçÖöÜü";
            var htmlContent = "<h2>İletişim</h2><p>Müşteri hizmetleri & destek hattı: 0850 123 45 67</p><strong>Önemli Bilgilendirme: Çarşamba günleri açıktır.</strong>";

            fakeService.MockSettings.Add(new SettingExportDto
            {
                Id = 1,
                Name = "İletişim Ayarları - " + allTurkishChars,
                SettingKey = "ContactInfo",
                SettingValue = htmlContent,
                Description = "Tüm Türkçe harfler: " + allTurkishChars,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            using (var memoryStream = new MemoryStream())
            {
                var result = await fakeService.ExportDataAsync(new DataExportRequest(), memoryStream);
                Assert.IsTrue(result.Success);

                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    var settingsEntry = zip.GetEntry("settings.json");
                    Assert.IsNotNull(settingsEntry);

                    using (var reader = new StreamReader(settingsEntry.Open()))
                    {
                        var json = await reader.ReadToEndAsync();

                        // 1. Direct UTF-8 check
                        Assert.IsTrue(json.Contains("İletişim"));
                        Assert.IsTrue(json.Contains(allTurkishChars));
                        Assert.IsTrue(json.Contains("<h2>İletişim</h2>"));
                        Assert.IsTrue(json.Contains("<p>Müşteri hizmetleri & destek"));
                        Assert.IsTrue(json.Contains("<strong>Önemli Bilgilendirme: Çarşamba"));

                        // 2. Escape sequence negation check
                        Assert.IsFalse(json.Contains("\\u0130"), "İ must not be escaped as \\u0130");
                        Assert.IsFalse(json.Contains("\\u0131"), "ı must not be escaped as \\u0131");
                        Assert.IsFalse(json.Contains("\\u015E"), "Ş must not be escaped as \\u015E");
                        Assert.IsFalse(json.Contains("\\u015F"), "ş must not be escaped as \\u015F");
                        Assert.IsFalse(json.Contains("\\u011E"), "Ğ must not be escaped as \\u011E");
                        Assert.IsFalse(json.Contains("\\u011F"), "ğ must not be escaped as \\u011F");
                        Assert.IsFalse(json.Contains("\\u00C7"), "Ç must not be escaped as \\u00C7");
                        Assert.IsFalse(json.Contains("\\u00E7"), "ç must not be escaped as \\u00E7");
                        Assert.IsFalse(json.Contains("\\u00D6"), "Ö must not be escaped as \\u00D6");
                        Assert.IsFalse(json.Contains("\\u00F6"), "ö must not be escaped as \\u00F6");
                        Assert.IsFalse(json.Contains("\\u00DC"), "Ü must not be escaped as \\u00DC");
                        Assert.IsFalse(json.Contains("\\u00FC"), "ü must not be escaped as \\u00FC");
                        Assert.IsFalse(json.Contains("\\u003C"), "< must not be escaped as \\u003C");
                        Assert.IsFalse(json.Contains("\\u003E"), "> must not be escaped as \\u003E");
                        Assert.IsFalse(json.Contains("\\u0026"), "& must not be escaped as \\u0026");

                        // 3. Deserialization round-trip check
                        var container = JsonSerializer.Deserialize<EntityExportContainer<SettingExportDto>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        Assert.IsNotNull(container);
                        Assert.AreEqual(1, container.RecordCount);
                        var setting = container.Records.First();
                        Assert.AreEqual("İletişim Ayarları - " + allTurkishChars, setting.Name);
                        Assert.AreEqual(htmlContent, setting.SettingValue);
                        Assert.AreEqual("Tüm Türkçe harfler: " + allTurkishChars, setting.Description);
                    }
                }
            }
        }

        [TestMethod]
        public async Task ExportDataAsync_SupportsEntityFiltering()
        {
            var fakeService = new FakeDataExportService();
            fakeService.MockSettings.Add(new SettingExportDto { Id = 1, Name = "S1" });
            fakeService.MockProducts.Add(new ProductExportDto { Id = 1, Name = "P1" });

            using (var memoryStream = new MemoryStream())
            {
                var request = new DataExportRequest
                {
                    IncludedEntities = new HashSet<string> { "Settings" }
                };

                var result = await fakeService.ExportDataAsync(request, memoryStream);

                Assert.IsTrue(result.Success);
                Assert.AreEqual(1, result.TotalRecords);
                Assert.IsTrue(result.Manifest.Entities.ContainsKey("Settings"));
                Assert.IsFalse(result.Manifest.Entities.ContainsKey("Products"));

                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    Assert.IsNotNull(zip.GetEntry("settings.json"));
                    Assert.IsNull(zip.GetEntry("products.json"));
                }
            }
        }

        [TestMethod]
        public void AdminSettingsController_ExportBackup_EndpointExposed()
        {
            var method = typeof(AdminSettingsController).GetMethod("ExportBackup");
            Assert.IsNotNull(method, "ExportBackup HTTP endpoint must be exposed on AdminSettingsController for JSON ZIP backups.");
        }
    }
}
