using EImece.Domain.Helpers.EmailHelper;
using Microsoft.AspNet.Identity;
using EImece.Domain.DependencyInjection;
using NLog;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class EmailService : IIdentityMessageService
    {
        [Inject]
        public IEmailSender EmailSender { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            EmailSender.SendEmail(message.Destination, message.Subject, message.Body);
            return Task.FromResult(0);
        }
    }
}