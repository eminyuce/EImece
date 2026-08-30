using EImece.Domain.Helpers.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class ProductFile : BaseEntity
    {
        public int FileStorageId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public FileStorage FileStorage { get; set; }
        public Product Product { get; set; }

        public string ImageFullPath(int width, int height, bool isThump = false)
        {
            var baseurl = EntityExtension.GetAbsoluteApplicationBaseUrl();
            var fileStorageId = FileStorage != null ? FileStorage.Id : 0;
            var result = this.GetCroppedImageUrl(fileStorageId, width, height, true, isThump) ?? string.Empty;
            if (!string.IsNullOrEmpty(baseurl) && !result.Contains(baseurl))
            {
                result = baseurl + result;
            }
            return result;
        }
    }
}