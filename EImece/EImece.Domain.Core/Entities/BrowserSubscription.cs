namespace EImece.Domain.Core.Entities;

public class BrowserSubscription : BaseEntity
{
    public string? Subject { get; set; }
    public int BrowserType { get; set; }
}
