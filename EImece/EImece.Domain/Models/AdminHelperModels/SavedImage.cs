using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
