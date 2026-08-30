using EImece.Domain.Helpers.EmailHelper;
using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class EmailService : IIdentityMessageService
    {
        private readonly IEmailSender EmailSender;

        public EmailService(IEmailSender emailSender)
        {
            EmailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Task SendAsync(IdentityMessage message)
        {
            // Identity email (confirmation, password reset) must not block the request thread on
            // the SMTP round-trip. Resolve/build synchronously and defer only the send.
            EmailSender.SendEmailInBackground(message.Destination, message.Subject, message.Body);
            return Task.FromResult(0);
        }
    }
}