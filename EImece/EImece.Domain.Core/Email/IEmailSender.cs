namespace EImece.Domain.Core.Email;

public interface IEmailSender
{
    /// <summary>
    /// Sends HTML email via MailKit when Smtp:IsEnabled and Host are set;
    /// otherwise logs the message (Debug-friendly sink).
    /// </summary>
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
