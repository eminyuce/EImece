using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class CompressedImageExportService : ICompressedImageExportService
    {
        private readonly ILogger<CompressedImageExportService> _logger;

        private readonly IImageExportRepository _imageExportRepository;

        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
        };

        public CompressedImageExportService(IImageExportRepository imageExportRepository, ILogger<CompressedImageExportService> logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _imageExportRepository = imageExportRepository ?? throw new ArgumentNullException(nameof(imageExportRepository));
        }

        public async Task<ImageExportPackageResult> ExportCompressedImagesAsync(
            string mediaImagesDirectory = null,
            long jpegQuality = 70L,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string targetDirectory = ResolveMediaImagesDirectory(mediaImagesDirectory);
            var result = new ImageExportPackageResult
            {
                FileName = $"compressed_images_{DateTime.Now:yyyyMMdd_HHmmss}.zip",
                ContentType = MediaTypeNames.Application.Zip
            };

            if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
            {
                _logger.LogWarning("Media images directory '{0}' not found. Returning empty archive.", targetDirectory);
                result.ZipBytes = CreateZipWithMetadata(new List<ImageMetadataMapping>(), new Dictionary<string, byte[]>());
                return result;
            }

            // Get image files in the root of targetDirectory (ignoring subdirectories like thumbs)
            string[] allFiles = Directory.GetFiles(targetDirectory, "*.*", SearchOption.TopDirectoryOnly);
            var imageFiles = allFiles
                .Where(f => AllowedExtensions.Contains(Path.GetExtension(f)))
                .ToList();

            // Load database relations
            var databaseLookups = await LoadDatabaseRelationsAsync(cancellationToken).ConfigureAwait(false);

            // Compress images and build metadata
            var compressedEntries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            var mappings = new List<ImageMetadataMapping>();

            long totalOriginalBytes = 0;
            long totalCompressedBytes = 0;

            foreach (var filePath in imageFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string fileName = Path.GetFileName(filePath);
                long originalSize = 0;
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    originalSize = fileInfo.Length;
                }
                catch
                {
                    // Ignore file info error
                }

                totalOriginalBytes += originalSize;

                // Compress image to byte array
                var (compressedBytes, width, height) = CompressImageFile(filePath, jpegQuality);
                long compressedSize = compressedBytes != null ? compressedBytes.Length : originalSize;
                totalCompressedBytes += compressedSize;

                if (compressedBytes != null && compressedBytes.Length > 0)
                {
                    compressedEntries[fileName] = compressedBytes;
                }

                // Map metadata
                var mapping = BuildImageMetadata(fileName, filePath, originalSize, compressedSize, width, height, databaseLookups);
                mappings.Add(mapping);
            }

            result.TotalImageCount = mappings.Count;
            result.TotalOriginalSizeBytes = totalOriginalBytes;
            result.TotalCompressedSizeBytes = totalCompressedBytes;
            result.ZipBytes = CreateZipWithMetadata(mappings, compressedEntries);

            _logger.LogInformation("Successfully generated compressed images export ZIP with {0} images.", result.TotalImageCount);
            return result;
        }

        private string ResolveMediaImagesDirectory(string customDirectory)
        {
            if (!string.IsNullOrWhiteSpace(customDirectory))
            {
                return customDirectory;
            }

            try
            {
                return AppConfig.StorageRoot;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve AppConfig.StorageRoot, falling back to default relative path.");
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "media", "images");
            }
        }

        private (byte[] CompressedBytes, int? Width, int? Height) CompressImageFile(string filePath, long jpegQuality)
        {
            string ext = Path.GetExtension(filePath)?.ToLowerInvariant() ?? "";
            try
            {
                if (ext == ".jpg" || ext == ".jpeg")
                {
                    using (var originalImage = Image.FromFile(filePath))
                    {
                        int width = originalImage.Width;
                        int height = originalImage.Height;

                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                        if (jpgEncoder != null)
                        {
                            using (var encoderParams = new EncoderParameters(1))
                            using (var ms = new MemoryStream())
                            {
                                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality);
                                originalImage.Save(ms, jpgEncoder, encoderParams);
                                return (ms.ToArray(), width, height);
                            }
                        }
                        else
                        {
                            using (var ms = new MemoryStream())
                            {
                                originalImage.Save(ms, ImageFormat.Jpeg);
                                return (ms.ToArray(), width, height);
                            }
                        }
                    }
                }
                else if (ext == ".png" || ext == ".bmp" || ext == ".gif")
                {
                    using (var originalImage = Image.FromFile(filePath))
                    {
                        int width = originalImage.Width;
                        int height = originalImage.Height;

                        ImageFormat format = ext == ".png" ? ImageFormat.Png :
                                             ext == ".gif" ? ImageFormat.Gif : ImageFormat.Bmp;

                        using (var ms = new MemoryStream())
                        {
                            originalImage.Save(ms, format);
                            byte[] processed = ms.ToArray();
                            return (processed, width, height);
                        }
                    }
                }
                else
                {
                    // Format like .webp or other: fallback to direct file bytes
                    byte[] rawBytes = File.ReadAllBytes(filePath);
                    return (rawBytes, null, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to compress image '{0}', falling back to raw file content.", filePath);
                try
                {
                    byte[] rawBytes = File.ReadAllBytes(filePath);
                    return (rawBytes, null, null);
                }
                catch (Exception readEx)
                {
                    _logger.LogError(readEx, "Failed to read raw image file '{0}'.", filePath);
                    return (null, null, null);
                }
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private byte[] CreateZipWithMetadata(List<ImageMetadataMapping> mappings, Dictionary<string, byte[]> imageFiles)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // 1. Add images_mapping.json to the archive
                    var jsonEntry = zipArchive.CreateEntry("images_mapping.json", CompressionLevel.Optimal);
                    using (var entryStream = jsonEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        string json = JsonConvert.SerializeObject(mappings, Formatting.Indented);
                        writer.Write(json);
                    }

                    // 2. Add each compressed image
                    foreach (var kvp in imageFiles)
                    {
                        string fileName = kvp.Key;
                        byte[] fileBytes = kvp.Value;
                        if (fileBytes == null || fileBytes.Length == 0)
                        {
                            continue;
                        }

                        var imageEntry = zipArchive.CreateEntry(fileName, CompressionLevel.Optimal);
                        using (var entryStream = imageEntry.Open())
                        {
                            entryStream.Write(fileBytes, 0, fileBytes.Length);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        private async Task<DatabaseLookupData> LoadDatabaseRelationsAsync(CancellationToken cancellationToken)
        {
            var lookups = new DatabaseLookupData();

            try
            {
                // FileStorages
                var fileStorages = await _imageExportRepository.GetFileStoragesAsync(cancellationToken).ConfigureAwait(false);
                lookups.FileStoragesByFileName = fileStorages
                    .Where(f => !string.IsNullOrWhiteSpace(f.FileName))
                    .GroupBy(f => f.FileName.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                lookups.FileStoragesById = fileStorages
                    .ToDictionary(f => f.Id, f => f);

                // Products (MainImageId & ProductFiles)
                var products = await _imageExportRepository.GetProductImageInfosAsync(cancellationToken).ConfigureAwait(false);
                foreach (var p in products)
                {
                    if (p.MainImageId.HasValue && p.MainImageId.Value > 0)
                    {
                        lookups.AddRelation(p.MainImageId.Value, new ImageRelatedRecordRef
                        {
                            TableName = "Products",
                            RecordId = p.Id,
                            RelationType = "MainImage",
                            RecordTitle = p.Name,
                            AdditionalReferenceData = new Dictionary<string, object>
                            {
                                { "ProductCode", p.ProductCode },
                                { "ProductCategoryId", p.ProductCategoryId }
                            }
                        });
                    }
                }

                var productFiles = await _imageExportRepository.GetProductFileImageInfosAsync(cancellationToken).ConfigureAwait(false);
                foreach (var pf in productFiles)
                {
                    if (pf.FileStorageId > 0)
                    {
                        lookups.AddRelation(pf.FileStorageId, new ImageRelatedRecordRef
                        {
                            TableName = "Products",
                            RecordId = pf.ProductId,
                            RelationType = "ProductFile",
                            RecordTitle = pf.ProductName,
                            AdditionalReferenceData = new Dictionary<string, object>
                            {
                                { "ProductFileId", pf.Id },
                                { "ProductCode", pf.ProductCode }
                            }
                        });
                    }
                }

                // ProductCategories (MainImageId)
                var productCategories = await _imageExportRepository.GetProductCategoryImageInfosAsync(cancellationToken).ConfigureAwait(false);
                foreach (var pc in productCategories)
                {
                    if (pc.MainImageId.HasValue && pc.MainImageId.Value > 0)
                    {
                        lookups.AddRelation(pc.MainImageId.Value, new ImageRelatedRecordRef
                        {
                            TableName = "ProductCategories",
                            RecordId = pc.Id,
                            RelationType = "MainImage",
                            RecordTitle = pc.Name,
                            AdditionalReferenceData = new Dictionary<string, object>
                            {
                                { "ParentId", pc.ParentId }
                            }
                        });
                    }
                }

                // Menus (MainImageId & MenuFiles)
                var menus = await _imageExportRepository.GetMenuImageInfosAsync(cancellationToken).ConfigureAwait(false);
                foreach (var m in menus)
                {
                    if (m.MainImageId.HasValue && m.MainImageId.Value > 0)
                    {
                        lookups.AddRelation(m.MainImageId.Value, new ImageRelatedRecordRef
                        {
                            TableName = "Menus",
                            RecordId = m.Id,
                            RelationType = "MainImage",
                            RecordTitle = m.Name,
                            AdditionalReferenceData = new Dictionary<string, object>
                            {
                                { "MenuLink", m.MenuLink },
                                { "Link", m.Link }
                            }
                        });
                    }
                }

                var menuFiles = await _imageExportRepository.GetMenuFileImageInfosAsync(cancellationToken).ConfigureAwait(false);
                foreach (var mf in menuFiles)
                {
                    if (mf.FileStorageId > 0)
                    {
                        lookups.AddRelation(mf.FileStorageId, new ImageRelatedRecordRef
                        {
                            TableName = "Menus",
                            RecordId = mf.MenuId,
                            RelationType = "MenuFile",
                            RecordTitle = mf.MenuName,
                            AdditionalReferenceData = new Dictionary<string, object>
                            {
                                { "MenuFileId", mf.Id }
                            }
                        });
                    }
                }

                // Stories (MainImageId & StoryFiles)
                var stories = await _imageExportRepository.GetStoryImageInfosAsync(cancellationToken).ConfigureAwait(false);
                foreach (var s in stories)
                {
                    if (s.MainImageId.HasValue && s.MainImageId.Value > 0)
                    {
                        lookups.AddRelation(s.MainImageId.Value, new ImageRelatedRecordRef
                        {
                            TableName = "Stories",
                            RecordId = s.Id,
                            RelationType = "MainImage",
                            RecordTitle = s.Name,
                            AdditionalReferenceData = new Dictionary<string, object>
                            {
                                { "StoryCategoryId", s.StoryCategoryId }
                            }
                        });
                    }
                }

                var storyFiles = await _imageExportRepository.GetStoryFileImageInfosAsync(cancellationToken).ConfigureAwait(false);
                foreach (var sf in storyFiles)
                {
                    if (sf.FileStorageId > 0)
                    {
                        lookups.AddRelation(sf.FileStorageId, new ImageRelatedRecordRef
                        {
                            TableName = "Stories",
                            RecordId = sf.StoryId,
                            RelationType = "StoryFile",
                            AdditionalReferenceData = new Dictionary<string, object>
                            {
                                { "StoryFileId", sf.Id }
                            }
                        });
                    }
                }

                // StoryCategories (MainImageId)
                var storyCategories = await _imageExportRepository.GetStoryCategoryImageInfosAsync(cancellationToken).ConfigureAwait(false);
                foreach (var sc in storyCategories)
                {
                    if (sc.MainImageId.HasValue && sc.MainImageId.Value > 0)
                    {
                        lookups.AddRelation(sc.MainImageId.Value, new ImageRelatedRecordRef
                        {
                            TableName = "StoryCategories",
                            RecordId = sc.Id,
                            RelationType = "MainImage",
                            RecordTitle = sc.Name
                        });
                    }
                }

                // Brands (MainImageId)
                var brands = await _imageExportRepository.GetBrandImageInfosAsync(cancellationToken).ConfigureAwait(false);
                foreach (var b in brands)
                {
                    if (b.MainImageId.HasValue && b.MainImageId.Value > 0)
                    {
                        lookups.AddRelation(b.MainImageId.Value, new ImageRelatedRecordRef
                        {
                            TableName = "Brands",
                            RecordId = b.Id,
                            RelationType = "MainImage",
                            RecordTitle = b.Name
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading database relations for compressed image export.");
            }

            return lookups;
        }

        private ImageMetadataMapping BuildImageMetadata(
            string fileName,
            string fullPath,
            long originalSize,
            long compressedSize,
            int? width,
            int? height,
            DatabaseLookupData lookups)
        {
            var mapping = new ImageMetadataMapping
            {
                FileName = fileName,
                FilePath = "media/images/" + fileName,
                OriginalSizeBytes = originalSize,
                CompressedSizeBytes = compressedSize,
                Width = width,
                Height = height,
                MimeType = GetMimeTypeFromFileName(fileName)
            };

            FileStorage fileStorage = null;
            if (lookups.FileStoragesByFileName.TryGetValue(fileName, out fileStorage))
            {
                mapping.FileStorageId = fileStorage.Id;
                if (!string.IsNullOrWhiteSpace(fileStorage.MimeType))
                {
                    mapping.MimeType = fileStorage.MimeType;
                }
                if (!mapping.Width.HasValue && fileStorage.Width > 0)
                {
                    mapping.Width = fileStorage.Width;
                }
                if (!mapping.Height.HasValue && fileStorage.Height > 0)
                {
                    mapping.Height = fileStorage.Height;
                }

                List<ImageRelatedRecordRef> relations;
                if (lookups.RelationsByFileStorageId.TryGetValue(fileStorage.Id, out relations))
                {
                    mapping.RelatedRecords.AddRange(relations);
                }
            }

            return mapping;
        }

        private static string GetMimeTypeFromFileName(string fileName)
        {
            string ext = Path.GetExtension(fileName)?.ToLowerInvariant() ?? "";
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return MediaTypeNames.Image.Jpeg;
                case ".png":
                    return "image/png";
                case ".gif":
                    return MediaTypeNames.Image.Gif;
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                default:
                    return MediaTypeNames.Application.Octet;
            }
        }

        private class DatabaseLookupData
        {
            public Dictionary<string, FileStorage> FileStoragesByFileName { get; set; } = new Dictionary<string, FileStorage>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<int, FileStorage> FileStoragesById { get; set; } = new Dictionary<int, FileStorage>();
            public Dictionary<int, List<ImageRelatedRecordRef>> RelationsByFileStorageId { get; set; } = new Dictionary<int, List<ImageRelatedRecordRef>>();

            public void AddRelation(int fileStorageId, ImageRelatedRecordRef relation)
            {
                List<ImageRelatedRecordRef> list;
                if (!RelationsByFileStorageId.TryGetValue(fileStorageId, out list))
                {
                    list = new List<ImageRelatedRecordRef>();
                    RelationsByFileStorageId[fileStorageId] = list;
                }
                list.Add(relation);
            }
        }
    }
}
