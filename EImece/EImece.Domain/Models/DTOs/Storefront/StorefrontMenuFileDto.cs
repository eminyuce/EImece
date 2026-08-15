using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected menu file read model for storefront page galleries.
    /// </summary>
    public class StorefrontMenuFileDto
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int FileStorageId { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public bool IsActive { get; set; }

        public string GetCroppedImageUrl(int fileStorageId = 0, int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            int targetId = fileStorageId > 0 ? fileStorageId : FileStorageId;
            var dummy = new FileStorage { Id = targetId, FileName = FileName, Name = Name };
            return dummy.GetCroppedImageUrl(targetId, width, height, isFullPath, isThumb);
        }
    }
}
