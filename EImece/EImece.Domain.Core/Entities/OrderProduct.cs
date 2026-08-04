using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class OrderProduct : IEntity<int>
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }

    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal ProductSalePrice { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }

    public Order? Order { get; set; }
}
