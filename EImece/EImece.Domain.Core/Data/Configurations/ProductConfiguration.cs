using EImece.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EImece.Domain.Core.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.Discount).HasPrecision(18, 2);

        // Legacy column is database-computed (average rating). Do not send on insert/update.
        builder.Property(p => p.Rating)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValue(0d);

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.ProductCategory)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
