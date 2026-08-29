using EImece.Domain;
using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ImageStorageDeleteTests
    {
        [TestMethod]
        public void DeleteStoredImageFiles_RemovesMainThumbAndWebPSidecars()
        {
            string root = Path.Combine(Path.GetTempPath(), "eimece-media-delete-" + Path.GetRandomFileName());
            string thumbs = Path.Combine(root, "thumbs");
            Directory.CreateDirectory(thumbs);

            string fileName = "product_123.jpg";
            WriteDummy(Path.Combine(root, fileName));
            WriteDummy(Path.Combine(thumbs, "thb" + fileName));
            WriteDummy(Path.Combine(root, "product_123.webp"));
            WriteDummy(Path.Combine(thumbs, "thbproduct_123.webp"));

            FilesHelper.DeleteStoredImageFiles(root, fileName);

            Assert.IsFalse(File.Exists(Path.Combine(root, fileName)), "main image should be deleted");
            Assert.IsFalse(File.Exists(Path.Combine(thumbs, "thb" + fileName)), "thumb should be deleted");
            Assert.IsFalse(File.Exists(Path.Combine(root, "product_123.webp")), "webp sidecar should be deleted");
            Assert.IsFalse(File.Exists(Path.Combine(thumbs, "thbproduct_123.webp")), "webp thumb sidecar should be deleted");

            Directory.Delete(root, true);
        }

        [TestMethod]
        public void DeleteStoredImageFiles_MissingFiles_DoesNotThrow()
        {
            string root = Path.Combine(Path.GetTempPath(), "eimece-media-delete-missing-" + Path.GetRandomFileName());
            Directory.CreateDirectory(root);

            FilesHelper.DeleteStoredImageFiles(root, "does-not-exist.jpg");
            FilesHelper.DeleteStoredImageFiles(root, null);
            FilesHelper.DeleteStoredImageFiles(root, FilesHelper.EXTERNAL_IMAGE);

            Directory.Delete(root, true);
        }

        [TestMethod]
        public void DeleteStoredImageFiles_IgnoresPathTraversalAndUsesFileNameOnly()
        {
            string root = Path.Combine(Path.GetTempPath(), "eimece-media-delete-safe-" + Path.GetRandomFileName());
            Directory.CreateDirectory(root);
            string fileName = "safe_img.png";
            WriteDummy(Path.Combine(root, fileName));

            FilesHelper.DeleteStoredImageFiles(root, @"..\..\windows\" + fileName);

            Assert.IsFalse(File.Exists(Path.Combine(root, fileName)));
            Directory.Delete(root, true);
        }

        [TestMethod]
        public void NormalFileExists_DetectsMissingAndExistingFiles()
        {
            string root = Path.Combine(Path.GetTempPath(), "eimece-media-exists-" + Path.GetRandomFileName());
            Directory.CreateDirectory(root);

            var filesHelper = new FilesHelper
            {
                StorageRoot = root
            };

            string existingFile = "photo.jpg";
            WriteDummy(Path.Combine(root, existingFile));

            Assert.IsTrue(filesHelper.NormalFileExists(existingFile), "Existing file should return true.");
            Assert.IsFalse(filesHelper.NormalFileExists("non-existent.jpg"), "Missing file should return false.");
            Assert.IsFalse(filesHelper.NormalFileExists(null), "Null filename should return false.");
            Assert.IsFalse(filesHelper.NormalFileExists(""), "Empty filename should return false.");

            Directory.Delete(root, true);
        }

        [TestMethod]
        public void TryCombineStorageFilePath_NullRootOrFileName_ReturnsFalse()
        {
            string fullPath;
            Assert.IsFalse(FilesHelper.TryCombineStorageFilePath(null, "a.jpg", out fullPath));
            Assert.IsNull(fullPath);
            Assert.IsFalse(FilesHelper.TryCombineStorageFilePath(@"C:\media", null, out fullPath));
            Assert.IsFalse(FilesHelper.TryCombineStorageFilePath(@"C:\media", "", out fullPath));
        }

        [TestMethod]
        public void TryCombineStorageFilePath_UsesFileNameOnly()
        {
            string fullPath;
            Assert.IsTrue(FilesHelper.TryCombineStorageFilePath(@"C:\media", @"..\secret\photo.jpg", out fullPath));
            Assert.AreEqual(Path.Combine(@"C:\media", "photo.jpg"), fullPath);
        }

        [TestMethod]
        public void GetFileNames2_InitializesStorageRootWhenEmpty()
        {
            var helper = new FilesHelper();
            Assert.IsTrue(string.IsNullOrWhiteSpace(helper.StorageRoot));

            var names = helper.GetFileNames2("photo.jpg");

            Assert.IsFalse(string.IsNullOrWhiteSpace(helper.StorageRoot));
            Assert.IsFalse(string.IsNullOrWhiteSpace(names.Item1));
            Assert.AreEqual("photo.jpg", names.Item3);
            StringAssert.EndsWith(names.Item1, "photo.jpg");
        }

        [TestMethod]
        public void AppConfigStorageRoot_IsNeverNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(AppConfig.StorageRoot));
        }

        private static void WriteDummy(string path)
        {
            File.WriteAllText(path, "x", Encoding.ASCII);
        }
    }
}
