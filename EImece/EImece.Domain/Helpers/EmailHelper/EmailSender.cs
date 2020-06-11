using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
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

            try
            {


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
                    smtpClient.UseDefaultCredentials = emailAccount.UseDefaultCredentials;
                    smtpClient.Host = emailAccount.Host;
                    smtpClient.Port = emailAccount.Port;
                    smtpClient.EnableSsl = emailAccount.EnableSsl;
                    if (emailAccount.UseDefaultCredentials)
                        smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                    else
                        smtpClient.Credentials = new NetworkCredential(emailAccount.Username, emailAccount.Password);
                    smtpClient.Send(message);
                    Logger.Info("Email Body" + message.Body);
                    Logger.Trace("Email is sent to " + emailAccount.Username);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
        }
        public EmailAccount GetEmailAccount()
        {
            var emailAccount = new EmailAccount();
            emailAccount.Host = SettingService.GetSettingByKey("AdminEmailHost");
            emailAccount.Password = SettingService.GetSettingByKey("AdminEmailPassword");
            emailAccount.EnableSsl = SettingService.GetSettingByKey("AdminEmailEnableSsl").ToBool();
            emailAccount.Port = SettingService.GetSettingByKey("AdminEmailPort").ToInt();
            emailAccount.DisplayName = SettingService.GetSettingByKey("AdminEmailDisplayName");
            emailAccount.Email = SettingService.GetSettingByKey("AdminEmail");
            emailAccount.UseDefaultCredentials = SettingService.GetSettingByKey("AdminEmailUseDefaultCredentials").ToBool();
            emailAccount.Username = SettingService.GetSettingByKey("AdminUserName").ToStr();
            return emailAccount;
        }
        public void SendEmailContactingUs(ContactUsFormViewModel contact)
        {
            var emailAccount = GetEmailAccount();
            var fromAddress = SettingService.GetSettingByKey("AdminEmail");
            var fromAddressDisplayName = SettingService.GetSettingByKey("AdminEmailDisplayName");
            var from = new MailAddress(fromAddress, fromAddressDisplayName);
            var to = new MailAddress(contact.Email, contact.Name);
            SendEmail(emailAccount, "Contact Us", contact.Message, from, to);
        }

        public void SendForgotPasswordEmail(string destination, string subject, string body)
        {
            var emailAccount = GetEmailAccount();
            var fromAddress = SettingService.GetSettingByKey("AdminEmail");
            var fromAddressDisplayName = SettingService.GetSettingByKey("AdminEmailDisplayName");
            var from = new MailAddress(fromAddress, fromAddressDisplayName);
            var to = new MailAddress(destination);
            SendEmail(emailAccount, subject, body, from, to);
        }
    }
}
