using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected file data joining ProductFile and FileStorage (where FileStorage.IsActive).
    /// </summary>
    public class StorefrontProductFileDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int FileStorageId { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Position { get; set; }
        public bool IsActive { get; set; }

        public string ImageFullPath(int width = 0, int height = 0)
        {
            var dummy = new FileStorage { Id = FileStorageId, FileName = FileName, Name = FileName };
            return dummy.GetCroppedImageUrl(FileStorageId, width, height, true);
        }

        public string GetCroppedImageUrl(int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            var dummy = new FileStorage { Id = FileStorageId, FileName = FileName, Name = FileName };
            return dummy.GetCroppedImageUrl(FileStorageId, width, height, isFullPath, isThumb);
        }

        public string GetCroppedImageUrl(int fileStorageId, int width, int height)
        {
            var dummy = new FileStorage { Id = fileStorageId, FileName = FileName, Name = FileName };
            return dummy.GetCroppedImageUrl(fileStorageId, width, height, false, false);
        }

        public string GetResponsiveImageSrcSet(int width = 0, int height = 0)
        {
            var dummy = new FileStorage { Id = FileStorageId, FileName = FileName, Name = FileName };
            return dummy.GetResponsiveImageSrcSet(FileStorageId, width, height);
        }
    }
}
