using EImece.Domain.Core.Configuration;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using AuthenticationOptions = EImece.Domain.Core.Configuration.AuthenticationOptions;

namespace EImece.Web.DependencyInjection;

public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    /// ASP.NET Core Identity + cookie auth + role policies + config-gated external providers (Phase 5).
    /// Requires ApplicationDbContext from AddEImeceData.
    /// </summary>
    public static IServiceCollection AddEImeceIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
        var authOptions = configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>()
            ?? new AuthenticationOptions();
        var cookie = authOptions.Cookie;

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Parity with legacy ApplicationUserManager.
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = cookie.LoginPath;
            options.AccessDeniedPath = cookie.AccessDeniedPath;
            options.LogoutPath = cookie.LogoutPath;
            options.ExpireTimeSpan = TimeSpan.FromDays(Math.Max(1, cookie.ExpireDays));
            options.SlidingExpiration = cookie.SlidingExpiration;
            options.Cookie.Name = "EImece.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
            };
        });

        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromMinutes(
                Math.Max(1, cookie.SecurityStampValidationIntervalMinutes));
        });

        var authBuilder = services.AddAuthentication();

        if (authOptions.Google.IsConfigured)
        {
            authBuilder.AddGoogle(o =>
            {
                o.ClientId = authOptions.Google.ClientId;
                o.ClientSecret = authOptions.Google.ClientSecret;
            });
        }

        if (authOptions.Facebook.IsConfigured)
        {
            authBuilder.AddFacebook(o =>
            {
                o.AppId = authOptions.Facebook.ClientId;
                o.AppSecret = authOptions.Facebook.ClientSecret;
            });
        }

        if (authOptions.Microsoft.IsConfigured)
        {
            authBuilder.AddMicrosoftAccount(o =>
            {
                o.ClientId = authOptions.Microsoft.ClientId;
                o.ClientSecret = authOptions.Microsoft.ClientSecret;
            });
        }

        if (authOptions.Twitter.IsConfigured)
        {
            authBuilder.AddTwitter(o =>
            {
                o.ConsumerKey = authOptions.Twitter.ClientId;
                o.ConsumerSecret = authOptions.Twitter.ClientSecret;
            });
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.AdminOnly, p => p.RequireRole(RoleNames.Admin));
            options.AddPolicy(AuthPolicies.AdminOrEditor, p =>
                p.RequireRole(RoleNames.Admin, RoleNames.NormalUser));
            options.AddPolicy(AuthPolicies.CustomerOnly, p => p.RequireRole(RoleNames.Customer));
        });

        return services;
    }
}
