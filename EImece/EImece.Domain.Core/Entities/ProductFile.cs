using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class ProductFile : BaseEntity
{
    [ForeignKey(nameof(FileStorage))]
    public int FileStorageId { get; set; }

    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    public FileStorage? FileStorage { get; set; }
    public Product? Product { get; set; }
}
