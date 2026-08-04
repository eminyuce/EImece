using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EImece.Domain.Core.Data;

/// <summary>
/// Design-time factory for <c>dotnet ef migrations</c>.
/// Uses EIMECE_DB_CONNECTION_STRING when set; otherwise a local placeholder.
/// </summary>
public sealed class EImeceDbContextFactory : IDesignTimeDbContextFactory<EImeceDbContext>
{
    public EImeceDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("EIMECE_DB_CONNECTION_STRING")
            ?? "Server=localhost;Database=EImece;Trusted_Connection=False;User Id=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<EImeceDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new EImeceDbContext(options);
    }
}
