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
    }
}