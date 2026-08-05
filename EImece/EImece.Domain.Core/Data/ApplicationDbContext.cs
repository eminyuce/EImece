using EImece.Domain.Core.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EImece.Domain.Core.Data;

/// <summary>
/// ASP.NET Core Identity store (parallel to legacy IdentityDbContext&lt;ApplicationUser&gt;).
/// Shares the same SQL Server database / connection string as <see cref="EImeceDbContext"/>.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(256);
            entity.Property(u => u.LastName).HasMaxLength(256);
        });
    }
}
