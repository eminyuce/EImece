using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace EImece.Domain.Helpers.EmailHelper
{
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email
        /// </summary>
        void SendEmail(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null);

        /// <summary>
        /// Sends an email
        /// </summary>
        void SendEmail(EmailAccount emailAccount, string subject, string body,
            MailAddress from, MailAddress to,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null);

        void SendEmail(string destination, string subject, string body);

        /// <summary>
        /// Fire-and-forget send. Builds the addresses synchronously (so configuration errors
        /// surface to the caller) but defers the SMTP round-trip to a background work item when
        /// hosted under ASP.NET. The body must already be rendered and all arguments must be
        /// DbContext-free before calling.
        /// </summary>
        void SendEmailInBackground(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null);

        /// <summary>
        /// Fire-and-forget send that resolves the email account on the calling thread and defers
        /// only the SMTP round-trip. Intended for ASP.NET Identity messages (email confirmation,
        /// password reset).
        /// </summary>
        void SendEmailInBackground(string destination, string subject, string body);

        void SendRenderedEmailTemplateToCustomer(EmailAccount emailAccount, Tuple<string, RazorRenderResult, Customer> renderedEmailTemplate, bool sendInBackground = false);

        void SendRenderedEmailTemplateToAdminUsers(EmailAccount emailAccount, Tuple<string, RazorRenderResult, Customer> renderedEmailTemplate, bool sendInBackground = false);
    }
}