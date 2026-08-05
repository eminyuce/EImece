using EImece.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EImece.Domain.Core.Data.Configurations;

public sealed class OrderProductConfiguration : IEntityTypeConfiguration<OrderProduct>
{
    public void Configure(EntityTypeBuilder<OrderProduct> builder)
    {
        builder.Property(op => op.Price).HasPrecision(18, 2);
        builder.Property(op => op.ProductSalePrice).HasPrecision(18, 2);
        builder.Property(op => op.TotalPrice).HasPrecision(18, 2);
    }
}
