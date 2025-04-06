using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace EImece.MyConsole
{
    public static class ImageCompressor
    {
        //     ImageCompressor.CompressImagesInDirectory(
        //  inputImageDirectoryPath: @"C:\Users\YUCE\Downloads\test",
        //         outputDirectory: @"C:\Users\YUCE\Downloads\test\output",
        //         quality: 40L,
        //         newExtension: ".jpg",
        //         baseFileName: "product-name",
        //         newWidth: 300,           // Resize by width, height auto-calculated
        //         newHeight: null
        //      );

        /// <summary>
        /// Compresses and optionally resizes all image files in a given directory.
        /// Supports changing file names and extensions, and preserves aspect ratio if only width or height is provided.
        /// </summary>
        /// <param name="inputImageDirectoryPath">The full path to the input directory containing images.</param>
        /// <param name="outputDirectory">The directory where the compressed/resized images will be saved.</param>
        /// <param name="quality">
        /// JPEG compression quality (1–100). 
        /// Ignored for non-JPEG formats.
        /// Recommended: 40–70 for reasonable size/quality.
        /// </param>
        /// <param name="newExtension">
        /// Optional new file extension (e.g., ".jpg", ".png").
        /// Pass <c>null</c> to retain the original extension.
        /// </param>
        /// <param name="baseFileName">
        /// The base name for all output files. 
        /// Files will be saved as baseFileName-1.jpg, baseFileName-2.jpg, etc.
        /// </param>
        /// <param name="newWidth">
        /// Optional new width for resizing.
        /// If provided and <paramref name="newHeight"/> is null, height will be auto-calculated to maintain aspect ratio.
        /// </param>
        /// <param name="newHeight">
        /// Optional new height for resizing.
        /// If provided and <paramref name="newWidth"/> is null, width will be auto-calculated to maintain aspect ratio.
        /// </param>
        /// <remarks>
        /// If both <paramref name="newWidth"/> and <paramref name="newHeight"/> are null, the image will retain its original dimensions.
        /// Supported input formats: .jpg, .jpeg, .png, .bmp, .gif
        /// </remarks>

        public static void CompressImagesInDirectory(
            string inputImageDirectoryPath,
            string outputDirectory,
            long quality,
            string newExtension,    // Pass null to keep original extension
            string baseFileName,    // e.g., "photo" → photo-1.jpg
            int? newWidth = null,
            int? newHeight = null
        )
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string[] imageFiles = Directory.GetFiles(inputImageDirectoryPath, "*.*");
            int index = 1;

            foreach (string inputImagePath in imageFiles)
            {
                string ext = Path.GetExtension(inputImagePath).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".bmp" && ext != ".gif")
                {
                    continue;
                }

                try
                {
                    using (Image originalImage = Image.FromFile(inputImagePath))
                    {
                        int originalWidth = originalImage.Width;
                        int originalHeight = originalImage.Height;
                        int targetWidth = originalWidth;
                        int targetHeight = originalHeight;

                        // Only width is provided
                        if (newWidth.HasValue && !newHeight.HasValue)
                        {
                            targetWidth = newWidth.Value;
                            targetHeight = (int)(originalHeight * (targetWidth / (float)originalWidth));
                        }
                        // Only height is provided
                        else if (!newWidth.HasValue && newHeight.HasValue)
                        {
                            targetHeight = newHeight.Value;
                            targetWidth = (int)(originalWidth * (targetHeight / (float)originalHeight));
                        }
                        // Both are provided
                        else if (newWidth.HasValue && newHeight.HasValue)
                        {
                            targetWidth = newWidth.Value;
                            targetHeight = newHeight.Value;
                        }

                        using (Bitmap resizedImage = new Bitmap(originalImage, new Size(targetWidth, targetHeight)))
                        {
                            string targetExtension = string.IsNullOrEmpty(newExtension)
                                ? ext
                                : newExtension.ToLowerInvariant();

                            string outputFileName = string.Format("{0}-{1}{2}", baseFileName, index, targetExtension);
                            string outputPath = Path.Combine(outputDirectory, outputFileName);
                            ImageFormat format = GetImageFormatByExtension(targetExtension);

                            if (format.Guid == ImageFormat.Jpeg.Guid)
                            {
                                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                                EncoderParameters encoderParams = new EncoderParameters(1);
                                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                                resizedImage.Save(outputPath, jpgEncoder, encoderParams);
                            }
                            else
                            {
                                resizedImage.Save(outputPath, format);
                            }

                            Console.WriteLine("Saved: " + outputPath);
                        }
                    }

                    index++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to process " + inputImagePath + ": " + ex.Message);
                }
            }
        }

        private static ImageFormat GetImageFormatByExtension(string ext)
        {
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".gif":
                    return ImageFormat.Gif;
                case ".ico":
                    return ImageFormat.Icon;
                default:
                    return ImageFormat.Jpeg;
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
    }
}
