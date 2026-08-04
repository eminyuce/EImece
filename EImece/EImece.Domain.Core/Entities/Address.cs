namespace EImece.Domain.Core.Entities;

public class Address : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public int AddressType { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string? Street { get; set; }
    public string? District { get; set; }
}
