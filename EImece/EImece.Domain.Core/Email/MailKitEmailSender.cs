using EImece.Domain.Core.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EImece.Domain.Core.Email;

public sealed class MailKitEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IOptions<SmtpOptions> options, ILogger<MailKitEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (string.IsNullOrWhiteSpace(message.ToAddress))
        {
            return EmailSendResult.Fail("ToAddress is required.");
        }

        var fromAddress = string.IsNullOrWhiteSpace(message.FromAddress)
            ? _options.FromAddress
            : message.FromAddress;
        var fromName = string.IsNullOrWhiteSpace(message.FromName)
            ? _options.FromDisplayName
            : message.FromName;

        if (!_options.IsEnabled || string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(fromAddress))
        {
            _logger.LogInformation(
                "SMTP sink (not sent): To={To} Subject={Subject} BodyLength={Length}",
                message.ToAddress,
                message.Subject,
                message.HtmlBody?.Length ?? 0);
            return EmailSendResult.Ok(loggedOnly: true);
        }

        try
        {
            var mime = new MimeMessage();
            mime.From.Add(new MailboxAddress(fromName ?? string.Empty, fromAddress));
            mime.To.Add(new MailboxAddress(message.ToName ?? string.Empty, message.ToAddress));
            mime.Subject = message.Subject ?? string.Empty;

            if (message.Cc is not null)
            {
                foreach (var cc in message.Cc.Where(static a => !string.IsNullOrWhiteSpace(a)))
                {
                    mime.Cc.Add(MailboxAddress.Parse(cc.Trim()));
                }
            }

            if (message.Bcc is not null)
            {
                foreach (var bcc in message.Bcc.Where(static a => !string.IsNullOrWhiteSpace(a)))
                {
                    mime.Bcc.Add(MailboxAddress.Parse(bcc.Trim()));
                }
            }

            var builder = new BodyBuilder { HtmlBody = message.HtmlBody };
            if (!string.IsNullOrWhiteSpace(message.AttachmentPath) && File.Exists(message.AttachmentPath))
            {
                await using var attachmentStream = File.OpenRead(message.AttachmentPath);
                await builder.Attachments.AddAsync(
                    message.AttachmentFileName ?? Path.GetFileName(message.AttachmentPath),
                    attachmentStream,
                    cancellationToken).ConfigureAwait(false);
            }

            mime.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var secure = _options.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(_options.Host, _options.Port, secure, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken)
                    .ConfigureAwait(false);
            }

            await client.SendAsync(mime, cancellationToken).ConfigureAwait(false);
            await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Email sent via MailKit to {To}", message.ToAddress);
            return EmailSendResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MailKit send failed to {To}", message.ToAddress);
            return EmailSendResult.Fail(ex.Message);
        }
    }
}
