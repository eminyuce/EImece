using Microsoft.Extensions.Logging;
using EImece.Domain.Abstractions;
using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers.EmailHelper
{
    /// <summary>
    /// Email sender
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        private readonly ISettingService SettingService;
        private readonly IBackgroundWorkQueue BackgroundWorkQueue;

        public EmailSender(ISettingService settingService, ILogger<EmailSender> logger, IBackgroundWorkQueue backgroundWorkQueue = null)
         {
            SettingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            BackgroundWorkQueue = backgroundWorkQueue;
        }
        private const string FromAddressRequiredMessage = "From Address cannot be null";
        private const string FromDisplayNameRequiredMessage = "from Address DisplayName cannot be null";

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="emailAccount">Email account to use</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Body</param>
        /// <param name="fromAddress">From address</param>
        /// <param name="fromName">From display name</param>
        /// <param name="toAddress">To address</param>
        /// <param name="toName">To display name</param>
        /// <param name="bcc">BCC addresses list</param>
        /// <param name="cc">CC addresses list</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        [Timed("service.email_sender.send_email")]
        public virtual void SendEmail(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            SendEmail(emailAccount, subject, body,
                new MailAddress(fromAddress, fromName), new MailAddress(toAddress, toName),
                bcc, cc, attachmentFilePath, attachmentFileName);
        }

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="emailAccount">Email account to use</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Body</param>
        /// <param name="from">From address</param>
        /// <param name="to">To address</param>
        /// <param name="bcc">BCC addresses list</param>
        /// <param name="cc">CC addresses list</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        [Timed("service.email_sender.send_email_address")]
        public virtual void SendEmail(EmailAccount emailAccount, string subject, string body,
            MailAddress from, MailAddress to,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            if (emailAccount == null)
            {
                throw new ArgumentException("No email account is defined.");
            }
            // Dispose the message (and any attachment file handles) once the send completes.
            // This matters especially when the send runs on a background thread.
            using (var message = new MailMessage())
            {
                message.From = from;
                message.To.Add(to);
                if (bcc != null)
                {
                    foreach (var address in bcc.Where(bccValue => !String.IsNullOrWhiteSpace(bccValue)))
                    {
                        message.Bcc.Add(address.Trim());
                    }
                }
                if (cc != null)
                {
                    foreach (var address in cc.Where(ccValue => !String.IsNullOrWhiteSpace(ccValue)))
                    {
                        message.CC.Add(address.Trim());
                    }
                }
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //create  the file attachment for this e-mail message
                if (!String.IsNullOrEmpty(attachmentFilePath) &&
                    File.Exists(attachmentFilePath))
                {
                    var attachment = new Attachment(attachmentFilePath);
                    attachment.Name = attachmentFileName ?? Path.GetFileName(attachmentFilePath);
                    message.Attachments.Add(attachment);
                }
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.UseDefaultCredentials = emailAccount.UseDefaultCredentials;
                    smtpClient.Host = emailAccount.Host;
                    smtpClient.Port = emailAccount.Port;
                    smtpClient.EnableSsl = emailAccount.EnableSsl;
                    smtpClient.Credentials = new NetworkCredential(emailAccount.Username, emailAccount.Password);
                    _logger.LogDebug($"Sending email. Subject: '{subject}', To: '{to.Address}', Host: '{emailAccount.Host}:{emailAccount.Port}', SSL: {emailAccount.EnableSsl}");
                    smtpClient.Send(message);
                    _logger.LogInformation($"Email sent successfully. Subject: '{subject}', To: '{to.Address}'");
                }
            }
        }

        /// <summary>
        /// Fire-and-forget send. Builds the addresses synchronously (so configuration errors
        /// surface to the caller) but defers the SMTP round-trip to a background work item when
        /// hosted under ASP.NET. The body must already be rendered and all arguments must be
        /// DbContext-free value before invoking this method.
        /// </summary>
        [Timed("service.email_sender.send_in_background")]
        public virtual void SendEmailInBackground(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            if (emailAccount == null)
            {
                throw new ArgumentException("No email account is defined.");
            }
            var from = new MailAddress(fromAddress, fromName);
            var to = new MailAddress(toAddress, toName);
            QueueOrRunSend(
                () => SendEmail(emailAccount, subject, body, from, to, bcc, cc, attachmentFilePath, attachmentFileName),
                subject);
        }

        /// <summary>
        /// Fire-and-forget variant of <see cref="SendEmail(string, string, string)"/>. The email
        /// account is resolved on the calling (request) thread while the DbContext is still alive;
        /// only the SMTP send is deferred to a background work item.
        /// </summary>
        [Timed("service.email_sender.send_in_background_destination")]
        public virtual void SendEmailInBackground(string destination, string subject, string body)
        {
            var emailAccount = SettingService.GetEmailAccount();
            var fromAddress = emailAccount.Email;
            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException(FromAddressRequiredMessage);
            }
            var fromAddressDisplayName = emailAccount.DisplayName;
            if (string.IsNullOrEmpty(fromAddressDisplayName))
            {
                throw new ArgumentException(FromDisplayNameRequiredMessage);
            }
            var from = new MailAddress(fromAddress, fromAddressDisplayName);
            var to = new MailAddress(destination);
            QueueOrRunSend(() => SendEmail(emailAccount, subject, body, from, to), subject);
        }

        /// <summary>
        /// Dispatches the SMTP send off the request thread when running under ASP.NET, using
        /// HostingEnvironment.QueueBackgroundWorkItem so the runtime tracks the work and delays
        /// app-pool shutdown (up to ~90s) rather than losing the mail. Outside a hosted
        /// environment (console app, Quartz jobs, unit tests) QueueBackgroundWorkItem is
        /// unavailable, so it falls back to a synchronous send. Exceptions are logged, never
        /// propagated, because the caller has already returned to the user.
        /// </summary>
        private void QueueOrRunSend(Action sendAction, string subjectForLog)
        {
            if (BackgroundWorkQueue != null)
            {
                BackgroundWorkQueue.QueueBackgroundWorkItem(cancellationToken =>
                {
                    try
                    {
                        sendAction();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Background email send failed. Subject: " + subjectForLog);
                    }
                });
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        sendAction();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Background email send failed. Subject: " + subjectForLog);
                    }
                });
            }
        }

        [Timed("service.email_sender.send_email_destination")]
        public virtual void SendEmail(string destination, string subject, string body)
        {
            var emailAccount = SettingService.GetEmailAccount();
            SendEmail(destination, subject, body, emailAccount);
        }

        [Timed("service.email_sender.send_email_destination_account")]
        public virtual void SendEmail(string destination, string subject, string body, EmailAccount emailAccount)
        {
            var fromAddress = emailAccount.Email;
            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException(FromAddressRequiredMessage);
            }
            var fromAddressDisplayName = emailAccount.DisplayName;
            if (string.IsNullOrEmpty(fromAddressDisplayName))
            {
                throw new ArgumentException(FromDisplayNameRequiredMessage);
            }
            var from = new MailAddress(fromAddress, fromAddressDisplayName);
            var to = new MailAddress(destination);
            SendEmail(emailAccount, subject, body, from, to);
        }

        [Timed("service.email_sender.send_rendered_to_customer")]
        public virtual void SendRenderedEmailTemplateToCustomer(EmailAccount emailAccount, Tuple<string, RazorRenderResult, Customer> renderedEmailTemplate, bool sendInBackground = false)
        {
            if (renderedEmailTemplate == null || string.IsNullOrEmpty(renderedEmailTemplate.Item1) && renderedEmailTemplate.Item2 != null)
            {
                _logger.LogError("renderedEmailTemplate cannot be empty");
                return;
            }

            Customer customer = renderedEmailTemplate.Item3;
            if (emailAccount == null)
            {
                _logger.LogError("renderedEmailTemplate for emailAccount cannot be empty");
                return;
            }
            if (customer == null)
            {
                _logger.LogError("renderedEmailTemplate for customer cannot be empty");
                return;
            }

            if (renderedEmailTemplate.Item2.GeneralError != null)
            {
                _logger.LogError("renderedEmailTemplate cannot be empty");
                throw renderedEmailTemplate.Item2.GeneralError;
            }

            var fromAddress = emailAccount.Email;
            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException(FromAddressRequiredMessage);
            }
            var fromAddressDisplayName = emailAccount.DisplayName;
            if (string.IsNullOrEmpty(fromAddressDisplayName))
            {
                throw new ArgumentException(FromDisplayNameRequiredMessage);
            }
            var from = new MailAddress(fromAddress, fromAddressDisplayName);
            var to = new MailAddress(customer.Email, customer.FullName);

            _logger.LogDebug("Queuing customer email. Subject: '{0}', To: '{1}'", renderedEmailTemplate.Item1, to.Address);
            // Validation and address building above run synchronously (so config/render errors
            // surface to the caller); only the SMTP round-trip is deferred when requested.
            if (sendInBackground)
            {
                SendEmailInBackground(emailAccount, renderedEmailTemplate.Item1, renderedEmailTemplate.Item2.Result,
                    from.Address, from.DisplayName, to.Address, to.DisplayName);
            }
            else
            {
                SendEmail(emailAccount, renderedEmailTemplate.Item1, renderedEmailTemplate.Item2.Result, from, to);
            }
        }

        [Timed("service.email_sender.send_rendered_to_admin")]
        public virtual void SendRenderedEmailTemplateToAdminUsers(EmailAccount emailAccount, Tuple<string, RazorRenderResult, Customer> renderedEmailTemplate, bool sendInBackground = false)
        {
            if (renderedEmailTemplate == null || string.IsNullOrEmpty(renderedEmailTemplate.Item1) && renderedEmailTemplate.Item2 != null)
            {
                _logger.LogError("renderedEmailTemplate cannot be empty");
                return;
            }

            Customer customer = renderedEmailTemplate.Item3;
            if (emailAccount == null)
            {
                _logger.LogError("renderedEmailTemplate for emailAccount cannot be empty");
                return;
            }
            if (customer == null)
            {
                _logger.LogError("renderedEmailTemplate for customer cannot be empty");
                return;
            }

            if (renderedEmailTemplate.Item2.GeneralError != null)
            {
                _logger.LogError("renderedEmailTemplate cannot be empty");
                throw renderedEmailTemplate.Item2.GeneralError;
            }

            var fromAddress = emailAccount.Email;
            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException(FromAddressRequiredMessage);
            }
            var fromAddressDisplayName = emailAccount.DisplayName;
            if (string.IsNullOrEmpty(fromAddressDisplayName))
            {
                throw new ArgumentException(FromDisplayNameRequiredMessage);
            }
            var from = new MailAddress(fromAddress, fromAddressDisplayName);

            _logger.LogDebug("Queuing admin email. Subject: '{0}', To: '{1}'", renderedEmailTemplate.Item1, from.Address);
            // Validation and address building above run synchronously (so config/render errors
            // surface to the caller); only the SMTP round-trip is deferred when requested.
            if (sendInBackground)
            {
                SendEmailInBackground(emailAccount, renderedEmailTemplate.Item1, renderedEmailTemplate.Item2.Result,
                    from.Address, from.DisplayName, from.Address, from.DisplayName);
            }
            else
            {
                SendEmail(emailAccount, renderedEmailTemplate.Item1, renderedEmailTemplate.Item2.Result, from, from);
            }
        }
    }
}