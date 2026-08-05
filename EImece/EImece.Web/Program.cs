using System.Globalization;
using System.IO.Compression;
using EImece.Domain.Core.DependencyInjection;
using EImece.Domain.Core.Media;
using EImece.Web.DependencyInjection;
using EImece.Web.Infrastructure.Routing;
using EImece.Web.Infrastructure.StaticFiles;
using EImece.Web.Middleware;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("EImece.Web starting (Phase 9 testing/optimization/deployment)");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Connection string env override parity with legacy ConnectionStringProvider (must run before DI).
    var envConnection = Environment.GetEnvironmentVariable("EIMECE_DB_CONNECTION_STRING");
    if (!string.IsNullOrWhiteSpace(envConnection))
    {
        builder.Configuration["ConnectionStrings:EImeceDbConnection"] = envConnection;
    }

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Persist data-protection keys for Linux-friendly multi-instance hosting.
    var keysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
    Directory.CreateDirectory(keysPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .SetApplicationName("EImece.Web");

    // Reverse-proxy awareness (nginx / IIS / ARR / Traefik).
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        // Operators should restrict KnownProxies/Networks in hardened environments.
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        [
            "application/json",
            "image/svg+xml",
            "application/javascript"
        ]);
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
    builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

    // SEO: lowercase URLs + trailing slash (legacy RouteConfig parity).
    builder.Services.Configure<RouteOptions>(options =>
    {
        options.LowercaseUrls = true;
        options.AppendTrailingSlash = true;
    });

    // Microsoft.Extensions.DependencyInjection composition root (same DI stack as legacy after Ninject removal).
    builder.Services.AddEImeceCore(builder.Configuration);
    builder.Services.AddEImeceInfrastructure(builder.Configuration);
    builder.Services.AddEImeceData(builder.Configuration);
    builder.Services.AddEImeceIdentity(builder.Configuration);

    builder.Services.AddControllersWithViews()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problem = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred."
                };
                return new BadRequestObjectResult(problem);
            };
        });

    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.Cookie.Name = ".EImece.Session";
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
    builder.Services.AddResponseCaching();

    if (!builder.Environment.IsDevelopment())
    {
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
    }

    var defaultCulture = builder.Configuration["EImece:ApplicationLanguages"]?.Split(',')[0].Trim() ?? "tr-TR";
    var supportedCultures = new[] { new CultureInfo(defaultCulture), new CultureInfo("en-US") };
    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        options.DefaultRequestCulture = new RequestCulture(defaultCulture);
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
        options.RequestCultureProviders =
        [
            new CookieRequestCultureProvider { CookieName = RouteConstants.CultureCookieName },
            new AcceptLanguageHeaderRequestCultureProvider()
        ];
    });

    var app = builder.Build();

    // Ensure media folders exist (legacy ~/media/images + tempFiles).
    app.Services.GetRequiredService<IMediaFileService>().EnsureDirectories();

    app.UseForwardedHeaders();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseResponseCompression();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    // Legacy mstore / admin theme assets from ../EImece/Content and ../EImece/Scripts.
    app.UseLegacyThemeStaticFiles(app.Environment);
    app.UseResponseCaching();
    app.UseRequestLocalization();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseRouting();
    app.UseSession();
    app.UseAuthentication();
    app.UseMiddleware<BypassAdminAuthMiddleware>();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    app.MapEImeceSeoRoutes();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "EImece.Web stopped because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}

// Expose entry assembly for integration tests.
public partial class Program;
