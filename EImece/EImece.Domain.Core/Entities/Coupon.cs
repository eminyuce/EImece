namespace EImece.Domain.Core.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public int DiscountPercentage { get; set; }
    public int Discount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
