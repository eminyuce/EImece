using EImece.Tests.Infrastructure;
using EImece.Areas.Admin.Controllers;
using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Tests.Services
{
    [TestClass]
    public class CompressedImageExportServiceTests
    {
        private class FakeImageExportRepositoryProxy : RealProxy
        {
            public FakeImageExportRepositoryProxy() : base(typeof(IImageExportRepository))
            {
            }

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                object defaultResult = null;
                if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType.IsValueType)
                    {
                        defaultResult = Activator.CreateInstance(mi.ReturnType);
                    }
                    else if (typeof(System.Threading.Tasks.Task).IsAssignableFrom(mi.ReturnType))
                    {
                        var itemType = mi.ReturnType.IsGenericType
                            ? mi.ReturnType.GetGenericArguments()[0]
                            : typeof(object);
                        if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var emptyList = Activator.CreateInstance(itemType);
                            var taskType = typeof(System.Threading.Tasks.Task<>).MakeGenericType(itemType);
                            defaultResult = Activator.CreateInstance(taskType, emptyList);
                        }
                        else
                        {
                            defaultResult = Activator.CreateInstance(mi.ReturnType);
                        }
                    }
                }
                return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
            }

            public IImageExportRepository Repository => (IImageExportRepository)GetTransparentProxy();
        }

        private class FakeImageExportService : ICompressedImageExportService
        {
            public Task<ImageExportPackageResult> ExportCompressedImagesAsync(
                string mediaImagesDirectory = null,
                long jpegQuality = 70L,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(new ImageExportPackageResult
                {
                    ZipBytes = new byte[] { 0x50, 0x4B, 0x05, 0x06 }, // Zip header
                    FileName = "compressed_images_test.zip",
                    ContentType = "application/zip",
                    TotalImageCount = 1,
                    TotalOriginalSizeBytes = 100,
                    TotalCompressedSizeBytes = 80
                });
            }
        }

        private string _tempDirectory;

        [TestInitialize]
        public void Setup()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "EImece_ImageExportTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDirectory))
            {
                try
                {
                    Directory.Delete(_tempDirectory, true);
                }
                catch
                {
                    // Cleanup best effort
                }
            }
        }

        [TestMethod]
        public async Task ExportCompressedImagesAsync_WhenImagesExist_CreatesZipWithCompressedImagesAndJsonMapping()
        {
            // Arrange: create a sample JPEG and PNG in the temp directory
            string jpgPath = Path.Combine(_tempDirectory, "sample1.jpg");
            using (var bmp = new Bitmap(100, 100))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Blue);
                }
                bmp.Save(jpgPath, ImageFormat.Jpeg);
            }

            string pngPath = Path.Combine(_tempDirectory, "sample2.png");
            using (var bmp = new Bitmap(80, 80))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Red);
                }
                bmp.Save(pngPath, ImageFormat.Png);
            }

            var proxy = new FakeImageExportRepositoryProxy();
            var service = new CompressedImageExportService(proxy.Repository, TestNullLoggers.Create<CompressedImageExportService>());

            // Act
            var result = await service.ExportCompressedImagesAsync(_tempDirectory, 70L, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ZipBytes);
            Assert.IsTrue(result.ZipBytes.Length > 0);
            Assert.AreEqual(2, result.TotalImageCount);
            Assert.IsTrue(result.FileName.StartsWith("compressed_images_"));
            Assert.IsTrue(result.FileName.EndsWith(".zip"));

            // Verify ZIP contents
            using (var ms = new MemoryStream(result.ZipBytes))
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
            {
                var jsonEntry = archive.GetEntry("images_mapping.json");
                Assert.IsNotNull(jsonEntry, "images_mapping.json should be present in the ZIP archive.");

                using (var reader = new StreamReader(jsonEntry.Open()))
                {
                    string json = reader.ReadToEnd();
                    var mappings = JsonConvert.DeserializeObject<List<ImageMetadataMapping>>(json);
                    Assert.IsNotNull(mappings);
                    Assert.AreEqual(2, mappings.Count);

                    var item1 = mappings.Find(m => m.FileName == "sample1.jpg");
                    Assert.IsNotNull(item1);
                    Assert.AreEqual("image/jpeg", item1.MimeType);
                    Assert.AreEqual("media/images/sample1.jpg", item1.FilePath);
                    Assert.IsTrue(item1.OriginalSizeBytes > 0);
                    Assert.IsTrue(item1.CompressedSizeBytes > 0);

                    var item2 = mappings.Find(m => m.FileName == "sample2.png");
                    Assert.IsNotNull(item2);
                    Assert.AreEqual("image/png", item2.MimeType);
                }

                Assert.IsNotNull(archive.GetEntry("sample1.jpg"));
                Assert.IsNotNull(archive.GetEntry("sample2.png"));
            }
        }

        [TestMethod]
        public async Task ExportCompressedImagesAsync_IgnoresSubdirectoriesAndNonImageFiles()
        {
            // Arrange
            string jpgPath = Path.Combine(_tempDirectory, "valid.jpg");
            using (var bmp = new Bitmap(50, 50))
            {
                bmp.Save(jpgPath, ImageFormat.Jpeg);
            }

            // Non-image file
            File.WriteAllText(Path.Combine(_tempDirectory, "notes.txt"), "some text");

            // Subdirectory (like thumbs) with an image
            string subDir = Path.Combine(_tempDirectory, "thumbs");
            Directory.CreateDirectory(subDir);
            using (var bmp = new Bitmap(20, 20))
            {
                bmp.Save(Path.Combine(subDir, "thumb.jpg"), ImageFormat.Jpeg);
            }

            var proxy = new FakeImageExportRepositoryProxy();
            var service = new CompressedImageExportService(proxy.Repository, TestNullLoggers.Create<CompressedImageExportService>());

            // Act
            var result = await service.ExportCompressedImagesAsync(_tempDirectory, 70L, CancellationToken.None);

            // Assert
            Assert.AreEqual(1, result.TotalImageCount);

            using (var ms = new MemoryStream(result.ZipBytes))
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
            {
                Assert.IsNotNull(archive.GetEntry("valid.jpg"));
                Assert.IsNull(archive.GetEntry("notes.txt"));
                Assert.IsNull(archive.GetEntry("thumb.jpg"));
                Assert.IsNull(archive.GetEntry("thumbs/thumb.jpg"));
            }
        }

        [TestMethod]
        public async Task ExportCompressedImagesAsync_WhenDirectoryMissing_ReturnsEmptyZipGracefully()
        {
            // Arrange
            string nonExistentPath = Path.Combine(_tempDirectory, "non_existent_folder");
            var proxy = new FakeImageExportRepositoryProxy();
            var service = new CompressedImageExportService(proxy.Repository, TestNullLoggers.Create<CompressedImageExportService>());

            // Act
            var result = await service.ExportCompressedImagesAsync(nonExistentPath, 70L, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TotalImageCount);
            Assert.IsNotNull(result.ZipBytes);

            using (var ms = new MemoryStream(result.ZipBytes))
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
            {
                var jsonEntry = archive.GetEntry("images_mapping.json");
                Assert.IsNotNull(jsonEntry);
            }
        }

        private class FakeSettingServiceProxy : RealProxy
        {
            public FakeSettingServiceProxy() : base(typeof(ISettingService))
            {
            }

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                object defaultResult = null;
                if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType.IsValueType)
                    {
                        defaultResult = Activator.CreateInstance(mi.ReturnType);
                    }
                }
                return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
            }

            public ISettingService Service => (ISettingService)GetTransparentProxy();
        }

        [TestMethod]
        public async Task ImagesController_DownloadCompressedImages_ReturnsFileContentResult()
        {
            // Arrange
            var controller = new EImece.Areas.Admin.Controllers.ImagesController(
                new FakeSettingServiceProxy().Service,
                new FakeImageExportService(),
                new FilesHelper(null),
                TestNullLoggers.Create<ImagesController>());

            // Act
            var actionResult = await controller.DownloadCompressedImages(CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(FileContentResult));
            var fileResult = (FileContentResult)actionResult;
            Assert.AreEqual("application/zip", fileResult.ContentType);
            Assert.AreEqual("compressed_images_test.zip", fileResult.FileDownloadName);
            Assert.AreEqual(4, fileResult.FileContents.Length);
        }
    }
}
