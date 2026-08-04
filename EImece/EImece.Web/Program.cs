using EImece.Domain.Core.DependencyInjection;
using EImece.Domain.Core.Media;
using EImece.Web.DependencyInjection;
using EImece.Web.Middleware;
using Microsoft.AspNetCore.DataProtection;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("EImece.Web starting (Phase 5 authentication & security)");

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

    // Microsoft.Extensions.DependencyInjection composition root (same DI stack as legacy after Ninject removal).
    builder.Services.AddEImeceCore(builder.Configuration);
    builder.Services.AddEImeceInfrastructure(builder.Configuration);
    builder.Services.AddEImeceData(builder.Configuration);
    builder.Services.AddEImeceIdentity(builder.Configuration);
    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // Ensure media folders exist (legacy ~/media/images + tempFiles).
    app.Services.GetRequiredService<IMediaFileService>().EnsureDirectories();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseRouting();
    app.UseAuthentication();
    app.UseMiddleware<BypassAdminAuthMiddleware>();
    app.UseAuthorization();

    // Area route placeholders (Admin / Customers) — controllers migrate in Phase 6.
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

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

// Expose entry assembly for integration tests in later phases.
public partial class Program;
