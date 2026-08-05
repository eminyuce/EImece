using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EImece.Web.Tests;

public sealed class EImeceWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:EImeceDbConnection"] =
                    "Server=127.0.0.1,1;Database=EImece_Test;User Id=x;Password=x;Connect Timeout=1;TrustServerCertificate=True;",
                ["EImece:Domain"] = "localhost",
                ["Smtp:IsEnabled"] = "true",
                ["Smtp:Host"] = "",
                ["Smtp:FromAddress"] = "noreply@eimece.local",
                ["Iyzico:ApiKey"] = "",
                ["Iyzico:SecretKey"] = "",
                ["Observability:EnableRequestLogging"] = "false"
            });
        });
    }
}
