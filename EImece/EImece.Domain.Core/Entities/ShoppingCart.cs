namespace EImece.Domain.Core.Entities;

public class ShoppingCart : BaseEntity
{
    public string? OrderGuid { get; set; }
    public string? ShoppingCartJson { get; set; }
    public string? UserId { get; set; }
}
