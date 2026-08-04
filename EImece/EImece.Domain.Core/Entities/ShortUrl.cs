namespace EImece.Domain.Core.Entities;

public class ShortUrl : BaseEntity
{
    public string? UrlKey { get; set; }
    public string? Url { get; set; }
    public int RequestCount { get; set; }
}
