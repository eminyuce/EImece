using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class ProductComment : BaseEntity
{
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }
    public string? UserId { get; set; }
    public string? Review { get; set; }
    public string? Email { get; set; }
    public string? Subject { get; set; }
    public int Rating { get; set; }
    public Product? Product { get; set; }
}
