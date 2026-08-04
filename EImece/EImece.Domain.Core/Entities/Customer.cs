namespace EImece.Domain.Core.Entities;

public class Customer : BaseEntity
{
    public string? Surname { get; set; }
    public string? GsmNumber { get; set; }
    public string? Email { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Ip { get; set; }
    public int Gender { get; set; }
    public string? Street { get; set; }
    public string? Town { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public string? Description { get; set; }
    public string? Company { get; set; }
    public int CustomerType { get; set; }
}
