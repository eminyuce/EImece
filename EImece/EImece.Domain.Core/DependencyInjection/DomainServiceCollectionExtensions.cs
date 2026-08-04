using EImece.Domain.Core.Data;
using EImece.Domain.Core.Identity;
using EImece.Domain.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EImece.Domain.Core.DependencyInjection;

public static class DomainServiceCollectionExtensions
{
    /// <summary>
    /// Registers EF Core DbContexts and thin repositories using Microsoft.Extensions.DependencyInjection.
    /// </summary>
    public static IServiceCollection AddEImeceData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("EImeceDbConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'EImeceDbConnection' is missing. Set ConnectionStrings:EImeceDbConnection or EIMECE_DB_CONNECTION_STRING.");

        var commandTimeout = configuration.GetValue("EImece:DatabaseCommandTimeoutSeconds", 120);

        services.AddDbContext<EImeceDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.CommandTimeout(commandTimeout);
                sql.MigrationsAssembly(typeof(EImeceDbContext).Assembly.FullName);
            });
        });

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.CommandTimeout(commandTimeout);
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });
        });

        // Identity stores are registered here for model/DI readiness; cookie auth is Phase 5.
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));

        return services;
    }
}
