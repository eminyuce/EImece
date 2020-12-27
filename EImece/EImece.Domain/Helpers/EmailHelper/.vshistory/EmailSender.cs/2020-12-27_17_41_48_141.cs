using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace EImece.Domain.Helpers.EmailHelper
{
    /// <summary>
    /// Email sender
    /// </summary>
    public class EmailSender : IEmailSender
    {
        [Inject]
        public ISettingService SettingService { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        public void SendEmail(EmailAccount emailAccount, string subject, string body,
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
        public virtual void SendEmail(EmailAccount emailAccount, string subject, string body,
            MailAddress from, MailAddress to,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            if (emailAccount == null)
            {
                throw new ArgumentException("No email account is defined.");
            }
            var message = new MailMessage();
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
                attachment.ContentDisposition.CreationDate = File.GetCreationTime(attachmentFilePath);
                attachment.ContentDisposition.ModificationDate = File.GetLastWriteTime(attachmentFilePath);
                attachment.ContentDisposition.ReadDate = File.GetLastAccessTime(attachmentFilePath);
                if (!String.IsNullOrEmpty(attachmentFileName))
                {
                    attachment.Name = attachmentFileName;
                }
                message.Attachments.Add(attachment);
            }

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Host = emailAccount.Host;
                smtpClient.Port = emailAccount.Port;
                smtpClient.EnableSsl = emailAccount.EnableSsl;
                smtpClient.Credentials = new NetworkCredential(emailAccount.Username, emailAccount.Password);
                smtpClient.Send(message);
            }
        }

        public void SendEmail(string destination, string subject, string body)
        {
            var emailAccount = SettingService.GetEmailAccount();
            SendEmail(destination, subject, body, emailAccount);
        }

        public void SendEmail(string destination, string subject, string body, EmailAccount emailAccount)
        {
            var fromAddress = emailAccount.Email;
            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("From Address cannot be null");
            }
            var fromAddressDisplayName = emailAccount.DisplayName;
            if (string.IsNullOrEmpty(fromAddressDisplayName))
            {
                throw new ArgumentException("from Address DisplayName cannot be null");
            }
            var from = new MailAddress(fromAddress, fromAddressDisplayName);
            var to = new MailAddress(destination);
            SendEmail(emailAccount, subject, body, from, to);
        }

        public void SendRenderedEmailTemplateToCustomer(EmailAccount emailAccount, Tuple<string, RazorRenderResult, Customer> renderedEmailTemplate)
        {
            Customer customer = renderedEmailTemplate.Item3;
            if (emailAccount == null)
            {
                Logger.Error("renderedEmailTemplate for emailAccount cannot be empty");
                return;
            }
            if (customer == null)
            {
                Logger.Error("renderedEmailTemplate for customer cannot be empty");
                return;
            }

            if (renderedEmailTemplate == null || string.IsNullOrEmpty(renderedEmailTemplate.Item1)  && renderedEmailTemplate.Item2 != null)
            {
                Logger.Error("renderedEmailTemplate cannot be empty");
                return;
            }
            if (renderedEmailTemplate.Item2.GeneralError != null)
            {
                Logger.Error("renderedEmailTemplate cannot be empty");
                throw renderedEmailTemplate.Item2.GeneralError;
            }

            var fromAddress = emailAccount.Email;
            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("From Address cannot be null");
            }
            var fromAddressDisplayName = emailAccount.DisplayName;
            if (string.IsNullOrEmpty(fromAddressDisplayName))
            {
                throw new ArgumentException("from Address DisplayName cannot be null");
            }
            var from = new MailAddress(fromAddress, fromAddressDisplayName);
            var to = new MailAddress(customer.Email, customer.FullName);

            Logger.Info("emailAccount:" + emailAccount + " from:" + from + " to:" + to +" renderedEmailTemplate: " + renderedEmailTemplate.Item1 + " " + renderedEmailTemplate.Item2);
            SendEmail(emailAccount, renderedEmailTemplate.Item1, renderedEmailTemplate.Item2.Result, from, to);
        }
        public void SendRenderedEmailTemplateToAdminUsers(EmailAccount emailAccount, Tuple<string, RazorRenderResult, Customer> renderedEmailTemplate)
        {
            Customer customer = renderedEmailTemplate.Item3;
            if (emailAccount == null)
            {
                Logger.Error("renderedEmailTemplate for emailAccount cannot be empty");
                return;
            }
            if (customer == null)
            {
                Logger.Error("renderedEmailTemplate for customer cannot be empty");
                return;
            }

            if (renderedEmailTemplate == null || string.IsNullOrEmpty(renderedEmailTemplate.Item1) && renderedEmailTemplate.Item2 != null)
            {
                Logger.Error("renderedEmailTemplate cannot be empty");
                return;
            }
            if (renderedEmailTemplate.Item2.GeneralError != null)
            {
                Logger.Error("renderedEmailTemplate cannot be empty");
                throw renderedEmailTemplate.Item2.GeneralError;
            }

            var fromAddress = emailAccount.Email;
            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("From Address cannot be null");
            }
            var fromAddressDisplayName = emailAccount.DisplayName;
            if (string.IsNullOrEmpty(fromAddressDisplayName))
            {
                throw new ArgumentException("from Address DisplayName cannot be null");
            }
            var from = new MailAddress(fromAddress, fromAddressDisplayName);
            var to = new MailAddress(customer.Email, customer.FullName);

            Logger.Info("emailAccount:" + emailAccount + " from:" + from + " to:" + to + " renderedEmailTemplate: " + renderedEmailTemplate.Item1 + " " + renderedEmailTemplate.Item2);
            SendEmail(emailAccount, renderedEmailTemplate.Item1, renderedEmailTemplate.Item2.Result, from, to);
        }

    }
}