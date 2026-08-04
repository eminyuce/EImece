namespace EImece.Domain.Core.Entities;

public class Faq : BaseEntity
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string? AddUserId { get; set; }
    public string? UpdateUserId { get; set; }
}
