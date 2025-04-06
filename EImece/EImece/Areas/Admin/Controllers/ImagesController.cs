using EImece.Domain.Helpers;
using System;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Collections.Generic;
using System.IO.Compression;

namespace EImece.Areas.Admin.Controllers
{
    public class ImagesController : BaseAdminController
    {
        // Get method to resize and display image
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index(string id, int width = 0, int height = 0)
        {
            var fileStorageId = id.Replace(".jpg", "").ToInt();
            var imageByte = FilesHelper.GetResizedImage(fileStorageId, width, height);
            if (imageByte != null)
            {
                return File(imageByte.ImageBytes, imageByte.ContentType);
            }
            else
            {
                return new EmptyResult();
            }
        }

        public ActionResult DownloadCompressedImages()
        {
            string outputDirectory = Path.Combine(Server.MapPath("~/media"), "compressed_images");

            if (!Directory.Exists(outputDirectory))
            {
                ViewBag.Message = "No compressed images found.";
                return View("CompressImages");
            }

            // Create a memory stream to hold the zip data
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Create a zip archive in memory
                using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // Iterate through the files in the directory and add them to the zip
                    foreach (string filePath in Directory.GetFiles(outputDirectory))
                    {
                        // Get the file name from the path
                        string fileName1 = Path.GetFileName(filePath);

                        // Add the file to the zip
                        ZipArchiveEntry zipEntry = zipArchive.CreateEntry(fileName1);

                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        using (Stream zipStream = zipEntry.Open())
                        {
                            fileStream.CopyTo(zipStream); // Copy file content to zip entry
                        }
                    }
                }

                // Set the position to the beginning of the memory stream before returning it
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Get the current timestamp in a suitable format (e.g., "yyyyMMdd_HHmmss")
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Generate the filename with the timestamp
                string fileName = $"compressed_images_{timestamp}.zip";

                // Return the memory stream as a file download with the dynamic filename
                return File(memoryStream.ToArray(), "application/zip", fileName);
            }
        }

        public ActionResult RemoveCompressedImages()
        {
            string mediaPath = Server.MapPath("~/media");
            string outputDirectory = Path.Combine(mediaPath, "compressed_images");
            string inputsDirectory = Path.Combine(mediaPath, "inputs_images");

            // Attempt to clear both directories
            bool outputCleared = ClearDirectory(outputDirectory, "No compressed images found to remove.");
            bool inputsCleared = ClearDirectory(inputsDirectory, "No input images found to remove.");

            // Set the success message if either directory was cleared
            if (outputCleared || inputsCleared)
            {
                ViewBag.Message = "Compressed images have been removed.";
            }

            return View("CompressImages");
        }

        // Helper method to clear a directory and handle messaging
        private bool ClearDirectory(string directoryPath, string notFoundMessage)
        {
            if (!Directory.Exists(directoryPath))
            {
                ViewBag.Message = notFoundMessage;
                return false;
            }

            foreach (var file in Directory.GetFiles(directoryPath))
            {
                System.IO.File.Delete(file);
            }

            return true;
        }



        // Get method to display image compression form
        [HttpGet]
        public ActionResult CompressImages()
        {
            return View();
        }

        // Post method to handle image compression
        [HttpPost]
        public ActionResult CompressImages(List<HttpPostedFileBase> file, string quality, string newExtension, string baseFileName, int? newWidth, int? newHeight)
        {
            if (file == null || file.Count == 0)
            {
                ViewBag.Message = "Please select at least one valid image file.";
                return View("CompressImages");
            }

            string mediaDirectory = Server.MapPath("~/media");
            string inputFileDirectory = "";
            if (!Directory.Exists(mediaDirectory))
            {
                ViewBag.Message = "Media directory does not exist. Please create it.";
                return View("CompressImages");
            }
            else
            {
                inputFileDirectory = Path.Combine(mediaDirectory, "inputs_images");
                if (!Directory.Exists(inputFileDirectory))
                {
                    Directory.CreateDirectory(inputFileDirectory);
                }
            }

            string outputDirectory = Path.Combine(mediaDirectory, "compressed_images");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            long qualityValue = string.IsNullOrEmpty(quality) ? 50L : System.Convert.ToInt64(quality);
            List<string> savedFiles = new List<string>();

            try
            {
                List<string> messages = new List<string>();
                foreach (HttpPostedFileBase uploadedFile in file)
                {
                    // Sanitize filename to avoid path issues
                    string fileName = Path.GetFileName(uploadedFile.FileName);
                    string ext = Path.GetExtension(fileName).ToLowerInvariant();
                    if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".bmp" && ext != ".gif")
                    {
                        continue;
                    }
                    string inputFilePath = Path.Combine(inputFileDirectory, fileName);

                    // Save the uploaded file to the media directory
                    using (var stream = new FileStream(inputFilePath, FileMode.Create))
                    {
                        uploadedFile.InputStream.CopyTo(stream);
                    }

                    savedFiles.Add(inputFilePath);
                }

                // Compress all saved files at once
                List<string> outputCompressedImagePaths = ImageCompressor.CompressImagesInDirectory(
                    inputImageDirectoryPath: inputFileDirectory, // Use the media directory with all files
                    outputDirectory: outputDirectory,
                    quality: qualityValue,
                    newExtension: newExtension,
                    baseFileName: baseFileName,
                    newWidth: newWidth,
                    newHeight: newHeight
                );

                ViewBag.Message = "Images compressed successfully!";
                ViewBag.ResultMessage = outputCompressedImagePaths;
                return View("CompressImages");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
                RemoveCompressedImages();
                return View("CompressImages");
            }
        }
    }
}
