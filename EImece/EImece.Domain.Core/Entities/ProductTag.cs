using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class ProductTag : IEntity<int>
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Tag))]
    public int TagId { get; set; }

    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    public Tag? Tag { get; set; }
    public Product? Product { get; set; }
}
