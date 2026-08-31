using Microsoft.Extensions.Logging;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.ExportImport
{
    public class DataExportService : IDataExportService
    {
        private readonly ILogger<DataExportService> _logger;

        private readonly IDataExportRepository Repository;
        private readonly IUsersService UsersService;

        public DataExportService(IDataExportRepository repository, IUsersService usersService, ILogger<DataExportService> logger)
         {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            UsersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        }

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
            var stopwatch = Stopwatch.StartNew();
            var result = new DataExportResult();
            var metadata = new ExportMetadata
            {
                ExportedBy = request.ExportedBy ?? "Administrator",
                CreatedAtUtc = DateTime.UtcNow,
                Application = "EImece",
                ApplicationVersion = "1.0"
            };

            var manifest = new ExportManifest
            {
                Application = "EImece",
                Format = "application-data-export",
                FormatVersion = "1.0",
                DatabaseProvider = DetectDatabaseProvider(),
                CreatedAtUtc = DateTime.UtcNow,
                Environment = System.Environment.MachineName
            };

            PopulateExcludedFields(manifest);

            var totalRecords = 0;
            var initialPosition = outputStream.CanSeek ? outputStream.Position : 0;

            try
            {
                using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    // 1. Settings
                    if (request.ShouldExport("Settings"))
                    {
                        var count = await ExportSettingsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Settings"] = new ExportEntityManifestEntry { File = "settings.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Settings");
                    }

                    // 2. MailTemplates
                    if (request.ShouldExport("MailTemplates"))
                    {
                        var count = await ExportMailTemplatesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["MailTemplates"] = new ExportEntityManifestEntry { File = "mail-templates.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("MailTemplates");
                    }

                    // 3. Faqs
                    if (request.ShouldExport("Faqs"))
                    {
                        var count = await ExportFaqsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Faqs"] = new ExportEntityManifestEntry { File = "faqs.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Faqs");
                    }

                    // 4. Subscribers
                    if (request.ShouldExport("Subscribers"))
                    {
                        var count = await ExportSubscribersAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Subscribers"] = new ExportEntityManifestEntry { File = "subscribers.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Subscribers");
                    }

                    // 5. FileStorages
                    if (request.ShouldExport("FileStorages"))
                    {
                        var count = await ExportFileStoragesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["FileStorages"] = new ExportEntityManifestEntry { File = "file-storages.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("FileStorages");
                    }

                    // 6. FileStorageTags
                    if (request.ShouldExport("FileStorageTags"))
                    {
                        var count = await ExportFileStorageTagsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["FileStorageTags"] = new ExportEntityManifestEntry { File = "file-storage-tags.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("FileStorageTags");
                    }

                    // 7. TagCategories
                    if (request.ShouldExport("TagCategories"))
                    {
                        var count = await ExportTagCategoriesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["TagCategories"] = new ExportEntityManifestEntry { File = "tag-categories.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("TagCategories");
                    }

                    // 8. Tags
                    if (request.ShouldExport("Tags"))
                    {
                        var count = await ExportTagsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Tags"] = new ExportEntityManifestEntry { File = "tags.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Tags");
                    }

                    // 9. Templates
                    if (request.ShouldExport("Templates"))
                    {
                        var count = await ExportTemplatesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Templates"] = new ExportEntityManifestEntry { File = "templates.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Templates");
                    }

                    // 10. Lists
                    if (request.ShouldExport("Lists"))
                    {
                        var count = await ExportListsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Lists"] = new ExportEntityManifestEntry { File = "lists.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Lists");
                    }

                    // 11. ListItems
                    if (request.ShouldExport("ListItems"))
                    {
                        var count = await ExportListItemsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["ListItems"] = new ExportEntityManifestEntry { File = "list-items.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("ListItems");
                    }

                    // 12. ProductCategories
                    if (request.ShouldExport("ProductCategories"))
                    {
                        var count = await ExportProductCategoriesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["ProductCategories"] = new ExportEntityManifestEntry { File = "product-categories.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("ProductCategories");
                    }

                    // 13. Brands
                    if (request.ShouldExport("Brands"))
                    {
                        var count = await ExportBrandsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Brands"] = new ExportEntityManifestEntry { File = "brands.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Brands");
                    }

                    // 14. Products
                    if (request.ShouldExport("Products"))
                    {
                        var count = await ExportProductsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Products"] = new ExportEntityManifestEntry { File = "products.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Products");
                    }

                    // 15. ProductSpecifications
                    if (request.ShouldExport("ProductSpecifications"))
                    {
                        var count = await ExportProductSpecificationsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["ProductSpecifications"] = new ExportEntityManifestEntry { File = "product-specifications.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("ProductSpecifications");
                    }

                    // 16. ProductFiles
                    if (request.ShouldExport("ProductFiles"))
                    {
                        var count = await ExportProductFilesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["ProductFiles"] = new ExportEntityManifestEntry { File = "product-files.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("ProductFiles");
                    }

                    // 17. ProductTags
                    if (request.ShouldExport("ProductTags"))
                    {
                        var count = await ExportProductTagsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["ProductTags"] = new ExportEntityManifestEntry { File = "product-tags.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("ProductTags");
                    }

                    // 18. ProductComments
                    if (request.ShouldExport("ProductComments"))
                    {
                        var count = await ExportProductCommentsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["ProductComments"] = new ExportEntityManifestEntry { File = "product-comments.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("ProductComments");
                    }

                    // 19. Coupons
                    if (request.ShouldExport("Coupons"))
                    {
                        var count = await ExportCouponsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Coupons"] = new ExportEntityManifestEntry { File = "coupons.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Coupons");
                    }

                    // 20. StoryCategories
                    if (request.ShouldExport("StoryCategories"))
                    {
                        var count = await ExportStoryCategoriesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["StoryCategories"] = new ExportEntityManifestEntry { File = "story-categories.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("StoryCategories");
                    }

                    // 21. Stories
                    if (request.ShouldExport("Stories"))
                    {
                        var count = await ExportStoriesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Stories"] = new ExportEntityManifestEntry { File = "stories.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Stories");
                    }

                    // 22. StoryFiles
                    if (request.ShouldExport("StoryFiles"))
                    {
                        var count = await ExportStoryFilesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["StoryFiles"] = new ExportEntityManifestEntry { File = "story-files.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("StoryFiles");
                    }

                    // 23. StoryTags
                    if (request.ShouldExport("StoryTags"))
                    {
                        var count = await ExportStoryTagsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["StoryTags"] = new ExportEntityManifestEntry { File = "story-tags.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("StoryTags");
                    }

                    // 24. Menus
                    if (request.ShouldExport("Menus"))
                    {
                        var count = await ExportMenusAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Menus"] = new ExportEntityManifestEntry { File = "menus.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Menus");
                    }

                    // 25. MenuFiles
                    if (request.ShouldExport("MenuFiles"))
                    {
                        var count = await ExportMenuFilesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["MenuFiles"] = new ExportEntityManifestEntry { File = "menu-files.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("MenuFiles");
                    }

                    // 26. MainPageImages
                    if (request.ShouldExport("MainPageImages"))
                    {
                        var count = await ExportMainPageImagesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["MainPageImages"] = new ExportEntityManifestEntry { File = "main-page-images.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("MainPageImages");
                    }

                    // 27. Customers
                    if (request.ShouldExport("Customers"))
                    {
                        var count = await ExportCustomersAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Customers"] = new ExportEntityManifestEntry { File = "customers.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Customers");
                    }

                    // 28. Addresses
                    if (request.ShouldExport("Addresses"))
                    {
                        var count = await ExportAddressesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Addresses"] = new ExportEntityManifestEntry { File = "addresses.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Addresses");
                    }

                    // 29. Orders
                    if (request.ShouldExport("Orders"))
                    {
                        var count = await ExportOrdersAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Orders"] = new ExportEntityManifestEntry { File = "orders.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Orders");
                    }

                    // 30. OrderProducts
                    if (request.ShouldExport("OrderProducts"))
                    {
                        var count = await ExportOrderProductsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["OrderProducts"] = new ExportEntityManifestEntry { File = "order-products.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("OrderProducts");
                    }

                    // 31. ShoppingCarts
                    if (request.ShouldExport("ShoppingCarts"))
                    {
                        var count = await ExportShoppingCartsAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["ShoppingCarts"] = new ExportEntityManifestEntry { File = "shopping-carts.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("ShoppingCarts");
                    }

                    // 32. Identity Users & Roles
                    if (request.ShouldExport("Users") && UsersService != null)
                    {
                        var count = await ExportUsersAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Users"] = new ExportEntityManifestEntry { File = "users.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Users");
                    }

                    if (request.ShouldExport("Roles") && UsersService != null)
                    {
                        var count = await ExportRolesAsync(archive, request.BatchSize, cancellationToken).ConfigureAwait(false);
                        manifest.Entities["Roles"] = new ExportEntityManifestEntry { File = "roles.json", RecordCount = count };
                        totalRecords += count;
                        metadata.IncludedEntities.Add("Roles");
                    }

                    // Manifest & Metadata JSON files
                    stopwatch.Stop();
                    metadata.DurationMs = stopwatch.ElapsedMilliseconds;
                    metadata.TotalRecords = totalRecords;

                    await WriteJsonEntryAsync(archive, "manifest.json", manifest, cancellationToken).ConfigureAwait(false);
                    await WriteJsonEntryAsync(archive, "metadata.json", metadata, cancellationToken).ConfigureAwait(false);
                }

                var finalPosition = outputStream.CanSeek ? outputStream.Position : 0;
                var compressedSize = outputStream.CanSeek ? (finalPosition - initialPosition) : 0;
                metadata.TotalSizeCompressedBytes = compressedSize;

                result.Success = true;
                result.TotalRecords = totalRecords;
                result.CompressedSizeBytes = compressedSize;
                result.Manifest = manifest;
                result.Metadata = metadata;
                result.Duration = stopwatch.Elapsed;

                _logger.LogInformation("Data export completed successfully. TotalRecords={0}, SizeBytes={1}, Duration={2}ms",
                    totalRecords, compressedSize, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Data export failed after {0}ms", stopwatch.ElapsedMilliseconds);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Duration = stopwatch.Elapsed;
                return result;
            }
        }

        public async Task<DataExportSummary> GetExportMetadataAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var summary = new DataExportSummary
            {
                Application = "EImece",
                DatabaseProvider = DetectDatabaseProvider()
            };

            if (Repository != null)
            {
                var counts = await Repository.GetEntityCountsAsync(cancellationToken).ConfigureAwait(false);
                foreach (var countEntry in counts)
                {
                    summary.EstimatedCounts[countEntry.Key] = countEntry.Value;
                }
            }

            if (UsersService != null)
            {
                summary.EstimatedCounts["Users"] = await UsersService.GetUsersCountAsync(cancellationToken).ConfigureAwait(false);
                summary.EstimatedCounts["Roles"] = await UsersService.GetRolesCountAsync(cancellationToken).ConfigureAwait(false);
            }

            summary.TotalEstimatedRecords = summary.EstimatedCounts.Values.Sum();
            return summary;
        }

        #region Entity Exporters

        private async Task<int> ExportSettingsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "settings.json", "Setting", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Setting>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new SettingExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    SettingKey = x.SettingKey,
                    SettingValue = x.SettingValue,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportMailTemplatesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "mail-templates.json", "MailTemplate", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<MailTemplate>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new MailTemplateExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Subject = x.Subject,
                    Body = x.Body,
                    TrackWithBitly = x.TrackWithBitly,
                    TrackWithMlnk = x.TrackWithMlnk,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportFaqsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "faqs.json", "Faq", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Faq>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new FaqExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Question = x.Question,
                    Answer = x.Answer,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportSubscribersAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "subscribers.json", "Subscriber", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Subscriber>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new SubscriberExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Note = x.Note,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportFileStoragesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "file-storages.json", "FileStorage", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<FileStorage>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new FileStorageExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    FileName = x.FileName,
                    FileUrl = x.FileUrl,
                    MimeType = x.MimeType,
                    FileSize = x.FileSize,
                    Width = x.Width,
                    Height = x.Height,
                    Type = x.Type,
                    IsFileExist = x.IsFileExist,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportFileStorageTagsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "file-storage-tags.json", "FileStorageTag", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<FileStorageTag>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new FileStorageTagExportDto
                {
                    Id = x.Id,
                    FileStorageId = x.FileStorageId,
                    TagId = x.TagId
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportTagCategoriesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "tag-categories.json", "TagCategory", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<TagCategory>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new TagCategoryExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportTagsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "tags.json", "Tag", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Tag>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new TagExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    TagCategoryId = x.TagCategoryId,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportTemplatesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "templates.json", "Template", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Template>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new TemplateExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    TemplateXml = x.TemplateXml,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportListsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "lists.json", "List", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<List>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new ListExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsService = x.IsService,
                    IsValues = x.IsValues,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportListItemsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "list-items.json", "ListItem", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<ListItem>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new ListItemExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ListId = x.ListId,
                    Value = x.Value,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportProductCategoriesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "product-categories.json", "ProductCategory", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<ProductCategory>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new ProductCategoryExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ShortDescription = x.ShortDescription,
                    ParentId = x.ParentId,
                    TemplateId = x.TemplateId,
                    MainImageId = x.MainImageId,
                    MainPage = x.MainPage,
                    DiscountPercantage = x.DiscountPercantage,
                    ImageState = x.ImageState,
                    MetaKeywords = x.MetaKeywords,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportBrandsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "brands.json", "Brand", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Brand>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new BrandExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    MainImageId = x.MainImageId,
                    MainPage = x.MainPage,
                    ImageState = x.ImageState,
                    MetaKeywords = x.MetaKeywords,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportProductsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "products.json", "Product", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Product>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new ProductExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    NameShort = x.NameShort,
                    NameLong = x.NameLong,
                    ProductCategoryId = x.ProductCategoryId,
                    BrandId = x.BrandId,
                    MainImageId = x.MainImageId,
                    ProductCode = x.ProductCode,
                    Price = x.Price,
                    Discount = x.Discount,
                    State = x.State,
                    MainPage = x.MainPage,
                    IsCampaign = x.IsCampaign,
                    ShortDescription = x.ShortDescription,
                    Description = x.Description,
                    VideoUrl = x.VideoUrl,
                    ProductColorOptions = x.ProductColorOptions,
                    ProductSizeOptions = x.ProductSizeOptions,
                    Rating = x.Rating,
                    ImageState = x.ImageState,
                    MetaKeywords = x.MetaKeywords,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportProductSpecificationsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "product-specifications.json", "ProductSpecification", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<ProductSpecification>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new ProductSpecificationExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductId = x.ProductId,
                    Value = x.Value,
                    Unit = x.Unit,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportProductFilesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "product-files.json", "ProductFile", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<ProductFile>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new ProductFileExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductId = x.ProductId,
                    FileStorageId = x.FileStorageId,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportProductTagsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "product-tags.json", "ProductTag", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<ProductTag>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new ProductTagExportDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    TagId = x.TagId
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportProductCommentsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "product-comments.json", "ProductComment", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<ProductComment>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new ProductCommentExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductId = x.ProductId,
                    UserId = x.UserId,
                    Review = x.Review,
                    Email = x.Email,
                    Subject = x.Subject,
                    Rating = x.Rating,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportCouponsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "coupons.json", "Coupon", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Coupon>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new CouponExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    DiscountPercentage = x.DiscountPercentage,
                    Discount = x.Discount,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportStoryCategoriesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "story-categories.json", "StoryCategory", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<StoryCategory>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new StoryCategoryExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    PageTheme = x.PageTheme,
                    MainImageId = x.MainImageId,
                    ImageState = x.ImageState,
                    MetaKeywords = x.MetaKeywords,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportStoriesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "stories.json", "Story", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Story>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new StoryExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    StoryCategoryId = x.StoryCategoryId,
                    Description = x.Description,
                    ShortDescription = x.ShortDescription,
                    AuthorName = x.AuthorName,
                    MainPage = x.MainPage,
                    IsFeaturedStory = x.IsFeaturedStory,
                    MainImageId = x.MainImageId,
                    ImageState = x.ImageState,
                    MetaKeywords = x.MetaKeywords,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportStoryFilesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "story-files.json", "StoryFile", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<StoryFile>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new StoryFileExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    StoryId = x.StoryId,
                    FileStorageId = x.FileStorageId,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportStoryTagsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "story-tags.json", "StoryTag", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<StoryTag>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new StoryTagExportDto
                {
                    Id = x.Id,
                    StoryId = x.StoryId,
                    TagId = x.TagId
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportMenusAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "menus.json", "Menu", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Menu>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new MenuExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ParentId = x.ParentId,
                    MenuLink = x.MenuLink,
                    Link = x.Link,
                    PageTheme = x.PageTheme,
                    LinkIsActive = x.LinkIsActive,
                    MainPage = x.MainPage,
                    MainImageId = x.MainImageId,
                    ImageState = x.ImageState,
                    MetaKeywords = x.MetaKeywords,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportMenuFilesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "menu-files.json", "MenuFile", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<MenuFile>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new MenuFileExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    MenuId = x.MenuId,
                    FileStorageId = x.FileStorageId,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportMainPageImagesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "main-page-images.json", "MainPageImage", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<MainPageImage>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new MainPageImageExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Link = x.Link,
                    MainImageId = x.MainImageId,
                    ImageState = x.ImageState,
                    MetaKeywords = x.MetaKeywords,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportCustomersAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "customers.json", "Customer", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Customer>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new CustomerExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Surname = x.Surname,
                    GsmNumber = x.GsmNumber,
                    Email = x.Email,
                    IdentityNumber = x.IdentityNumber,
                    UserId = x.UserId,
                    IsPermissionGranted = x.IsPermissionGranted,
                    Gender = x.Gender,
                    Street = x.Street,
                    Town = x.Town,
                    District = x.District,
                    City = x.City,
                    Country = x.Country,
                    ZipCode = x.ZipCode,
                    Description = x.Description,
                    Company = x.Company,
                    CustomerType = x.CustomerType,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportAddressesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "addresses.json", "Address", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Address>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new AddressExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    AddressType = x.AddressType,
                    City = x.City,
                    Country = x.Country,
                    ZipCode = x.ZipCode,
                    Street = x.Street,
                    District = x.District,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportOrdersAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "orders.json", "Order", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<Order>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new OrderExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    OrderNumber = x.OrderNumber,
                    OrderGuid = x.OrderGuid,
                    DeliveryDate = x.DeliveryDate,
                    UserId = x.UserId,
                    OrderType = x.OrderType,
                    OrderStatus = x.OrderStatus,
                    AdminOrderNote = x.AdminOrderNote,
                    OrderComments = x.OrderComments,
                    CargoPrice = x.CargoPrice,
                    ShippingAddressId = x.ShippingAddressId,
                    BillingAddressId = x.BillingAddressId,
                    Coupon = x.Coupon,
                    CouponDiscount = x.CouponDiscount,
                    Price = x.Price,
                    PaidPrice = x.PaidPrice,
                    Installment = x.Installment,
                    Currency = x.Currency,
                    PaymentStatus = x.PaymentStatus,
                    ShipmentTrackingNumber = x.ShipmentTrackingNumber,
                    ShipmentCompanyName = x.ShipmentCompanyName,
                    Locale = x.Locale,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportOrderProductsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "order-products.json", "OrderProduct", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<OrderProduct>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new OrderProductExportDto
                {
                    Id = x.Id,
                    OrderId = x.OrderId,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    TotalPrice = x.TotalPrice,
                    ProductSalePrice = x.ProductSalePrice,
                    ProductName = x.ProductName,
                    ProductCode = x.ProductCode,
                    CategoryName = x.CategoryName,
                    ProductSpecItems = x.ProductSpecItems,
                    ProductImageUrl = x.ProductImageUrl
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportShoppingCartsAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "shopping-carts.json", "ShoppingCart", batchSize, ct, async (skip, take) =>
            {
                var items = await Repository.GetPageAsync<ShoppingCart>(skip, take, ct).ConfigureAwait(false);
                return items.Select(x => new ShoppingCartExportDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    OrderGuid = x.OrderGuid,
                    ShoppingCartJson = x.ShoppingCartJson,
                    UserId = x.UserId,
                    IsActive = x.IsActive,
                    Position = x.Position,
                    Lang = x.Lang,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).ToList();
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportUsersAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "users.json", "User", batchSize, ct, async (skip, take) =>
            {
                return await UsersService.GetUsersForExportAsync(skip, take, ct).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private async Task<int> ExportRolesAsync(ZipArchive archive, int batchSize, CancellationToken ct)
        {
            return await StreamEntityExportAsync(archive, "roles.json", "Role", batchSize, ct, async (skip, take) =>
            {
                return await UsersService.GetRolesForExportAsync(skip, take, ct).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        #endregion

        #region Helper Methods

        private async Task<int> StreamEntityExportAsync<T>(
            ZipArchive archive,
            string fileName,
            string entityName,
            int batchSize,
            CancellationToken ct,
            Func<int, int, Task<List<T>>> batchFetcher)
        {
            var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
            var totalExported = 0;
            var skip = 0;

            using (var entryStream = entry.Open())
            using (var writer = new Utf8JsonWriter(entryStream, WriterOptions))
            {
                writer.WriteStartObject();
                writer.WriteString("entity", entityName);
                writer.WriteNumber("schemaVersion", 1);
                writer.WriteStartArray("records");

                while (true)
                {
                    ct.ThrowIfCancellationRequested();

                    var batch = await batchFetcher(skip, batchSize).ConfigureAwait(false);
                    if (batch == null || batch.Count == 0)
                    {
                        break;
                    }

                    foreach (var record in batch)
                    {
                        JsonSerializer.Serialize(writer, record, JsonOptions);
                        totalExported++;
                    }

                    skip += batch.Count;
                    if (batch.Count < batchSize)
                    {
                        break;
                    }
                }

                writer.WriteEndArray();
                writer.WriteNumber("recordCount", totalExported);
                writer.WriteEndObject();
                await writer.FlushAsync(ct).ConfigureAwait(false);
            }

            return totalExported;
        }

        private async Task WriteJsonEntryAsync<T>(ZipArchive archive, string fileName, T data, CancellationToken ct)
        {
            var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
            using (var stream = entry.Open())
            {
                await JsonSerializer.SerializeAsync(stream, data, JsonOptions, ct).ConfigureAwait(false);
            }
        }

        private string DetectDatabaseProvider()
        {
            try
            {
                var conn = ConnectionStringProvider.GetConnectionString();
                if (!string.IsNullOrEmpty(conn))
                {
                    return "SqlServer";
                }
            }
            catch
            {
                // Fallback
            }
            return "SqlServer";
        }

        private void PopulateExcludedFields(ExportManifest manifest)
        {
            manifest.ExcludedFields["Users"] = new List<string>
            {
                "PasswordHash",
                "SecurityStamp",
                "AuthenticatorKey",
                "TwoFactorTokens",
                "Logins",
                "Claims"
            };

            manifest.ExcludedFields["Orders"] = new List<string>
            {
                "CardToken",
                "CardUserKey",
                "BinNumber",
                "LastFourDigits",
                "CardType",
                "CardAssociation",
                "CardFamily",
                "AuthCode",
                "HostReference",
                "PaymentId",
                "Token",
                "ConversationId",
                "BasketId"
            };
        }

        #endregion
    }
}
