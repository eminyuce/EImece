using System;

namespace EImece.Domain.Models.AdminHelperModels
{
    public class SavedImage
    {
        public string NewFileName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ImageSize { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public int ThumpBitmapWidth { get; set; }
        public int ThumpBitmapHeight { get; set; }
        public byte[] ImageBytes { get; set; }
        public DateTime UpdatedDated { get; set; }


        public SavedImage()
        {
        }

        public SavedImage(string newFileName, int width, int height, int imageSize, string contentType, string fileName, string fileHash)
        {
            NewFileName = newFileName;
            Width = width;
            Height = height;
            ImageSize = imageSize;
            ContentType = contentType;
            FileName = fileName;
            FileHash = fileHash;
        }

        public SavedImage(int thumpBitmapWidth, int thumpBitmapHeight, int originalWidth, int originalHeight, string fileName)
        {
            ThumpBitmapWidth = thumpBitmapWidth;
            ThumpBitmapHeight = thumpBitmapHeight;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
            FileName = fileName;
        }

        public SavedImage(int width, int height, int originalWidth, int originalHeight)
        {
            Width = width;
            Height = height;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
        }

        public SavedImage(byte[] imageBytes, string contentType)
        {
            ImageBytes = imageBytes;
            ContentType = contentType;
        }
    }
}