namespace EImece.Domain.Core.Entities;

public class MailTemplate : BaseEntity
{
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? UpdateUserId { get; set; }
    public string? AddUserId { get; set; }
    public bool TrackWithBitly { get; set; }
    public bool TrackWithMlnk { get; set; }
}
