namespace EImece.Domain.Core.Email;

public sealed class EmailMessage
{
    public string ToAddress { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? FromAddress { get; set; }
    public string? FromName { get; set; }
    public IReadOnlyList<string>? Cc { get; set; }
    public IReadOnlyList<string>? Bcc { get; set; }
    public string? AttachmentPath { get; set; }
    public string? AttachmentFileName { get; set; }
}

public sealed class EmailSendResult
{
    public bool Sent { get; init; }
    public bool LoggedOnly { get; init; }
    public string? Error { get; init; }

    public static EmailSendResult Ok(bool loggedOnly = false) => new() { Sent = true, LoggedOnly = loggedOnly };
    public static EmailSendResult Fail(string error) => new() { Sent = false, Error = error };
}
