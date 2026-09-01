using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace EImece.Domain.Configuration
{
    public static class EimeceConfigurationRegistration
    {
        public static IConfiguration BuildConfiguration()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var environment = Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? "Production";

            var builder = new ConfigurationBuilder();

            if (Directory.Exists(basePath))
            {
                builder.SetBasePath(basePath);
            }

            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();

            return builder.Build();
        }

        public static IServiceCollection AddEimeceConfiguration(this IServiceCollection services, IConfiguration configuration = null)
        {
            var config = configuration ?? BuildConfiguration();
            services.AddSingleton<IConfiguration>(config);
            if (config is IConfigurationRoot root)
            {
                services.AddSingleton<IConfigurationRoot>(root);
            }

            return services;
        }
    }
}
