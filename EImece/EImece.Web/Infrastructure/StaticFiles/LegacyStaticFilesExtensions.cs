using Microsoft.Extensions.FileProviders;

namespace EImece.Web.Infrastructure.StaticFiles;

public static class LegacyStaticFilesExtensions
{
    /// <summary>
    /// Serves legacy theme assets from EImece/Content and EImece/Scripts without copying
    /// the full tree into wwwroot (Phase 7). Paths: /Content/*, /Scripts/*.
    /// </summary>
    public static IApplicationBuilder UseLegacyThemeStaticFiles(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        var legacyWebRoot = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "EImece"));
        var contentPath = Path.Combine(legacyWebRoot, "Content");
        var scriptsPath = Path.Combine(legacyWebRoot, "Scripts");

        if (Directory.Exists(contentPath))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(contentPath),
                RequestPath = "/Content"
            });
        }

        if (Directory.Exists(scriptsPath))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(scriptsPath),
                RequestPath = "/Scripts"
            });
        }

        return app;
    }
}
