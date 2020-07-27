using EImece.Domain.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System;

namespace EImece
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            //app.CreatePerOwinContext(ApplicationDbContext.Create);
            //app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            //app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            var microsoftAccountClientId = Domain.AppConfig.GetConfigString("MicrosoftAccount_ClientId");
            var microsoftAccountClientSecret = Domain.AppConfig.GetConfigString("MicrosoftAccount_ClientSecret");

            // Uncomment the following lines to enable logging in with third party login providers

            if (!String.IsNullOrEmpty(microsoftAccountClientId) && !String.IsNullOrEmpty(microsoftAccountClientSecret))
            {
                app.UseMicrosoftAccountAuthentication(
                   clientId: microsoftAccountClientId,
                   clientSecret: microsoftAccountClientSecret);
            }

            var twitterAccountConsumerKey = Domain.AppConfig.GetConfigString("TwitterAccount_ConsumerKey");
            var twitterAccountConsumerSecret = Domain.AppConfig.GetConfigString("TwitterAccount_ConsumerSecret");

            if (!String.IsNullOrEmpty(twitterAccountConsumerKey) && !String.IsNullOrEmpty(twitterAccountConsumerSecret))
            {
                app.UseTwitterAuthentication(
                       consumerKey: twitterAccountConsumerKey,
                       consumerSecret: twitterAccountConsumerSecret);
            }

            var facebookAccountAppId = Domain.AppConfig.GetConfigString("FacebookAccount_AppId");
            var facebookAccountAppSecret = Domain.AppConfig.GetConfigString("FacebookAccount_AppSecret");

            if (!String.IsNullOrEmpty(facebookAccountAppId) && !String.IsNullOrEmpty(facebookAccountAppSecret))
            {
                app.UseFacebookAuthentication(
                     appId: facebookAccountAppId,
                     appSecret: facebookAccountAppSecret);
            }

            var googleAccountClientId = Domain.AppConfig.GetConfigString("GoogleAccount_ClientId");
            var googleAccountClientSecret = Domain.AppConfig.GetConfigString("GoogleAccount_ClientSecret");

            if (!String.IsNullOrEmpty(googleAccountClientId) && !String.IsNullOrEmpty(googleAccountClientSecret))
            {
                app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
                {
                    ClientId = googleAccountClientId,
                    ClientSecret = googleAccountClientSecret
                });
            }
        }
    }
}