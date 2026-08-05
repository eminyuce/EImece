using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class ProductSpecification : BaseEntity
{
    public string? Value { get; set; }
    public string? Unit { get; set; }

    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    public Product? Product { get; set; }
}
