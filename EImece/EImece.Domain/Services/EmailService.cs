using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Helpers.EmailHelper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EImece.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class EmailService : IIdentityMessageService
    {
        [Inject]
        public IEmailSender EmailSender { get; set; }

        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            EmailSender.SendEmail(message.Destination, message.Subject, message.Body);
            return Task.FromResult(0);
        }
    }
}
