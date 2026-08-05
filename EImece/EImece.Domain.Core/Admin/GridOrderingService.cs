using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EImece.Domain.Core.Admin;

public interface IGridOrderingService
{
    Task ApplyAsync<T>(DbSet<T> set, List<OrderingItem> values, string? checkbox, CancellationToken ct = default)
        where T : BaseEntity;
}

public sealed class GridOrderingService : IGridOrderingService
{
    private readonly EImeceDbContext _db;

    public GridOrderingService(EImeceDbContext db) => _db = db;

    public async Task ApplyAsync<T>(DbSet<T> set, List<OrderingItem> values, string? checkbox, CancellationToken ct = default)
        where T : BaseEntity
    {
        if (values.Count == 0) return;
        var ids = values.Select(v => v.Id).ToList();
        var entities = await set.Where(e => ids.Contains(e.Id)).ToListAsync(ct).ConfigureAwait(false);
        foreach (var item in values)
        {
            var entity = entities.FirstOrDefault(e => e.Id == item.Id);
            if (entity is null) continue;

            if (string.IsNullOrEmpty(checkbox))
            {
                entity.Position = item.Position;
            }
            else if (checkbox.Equals("State", StringComparison.OrdinalIgnoreCase))
            {
                entity.IsActive = item.IsActive;
            }
            else if (checkbox.Equals("MainPage", StringComparison.OrdinalIgnoreCase))
            {
                switch (entity)
                {
                    case Product p: p.MainPage = item.IsActive; break;
                    case Story s: s.MainPage = item.IsActive; break;
                    case ProductCategory c: c.MainPage = item.IsActive; break;
                    case Brand b: b.MainPage = item.IsActive; break;
                    case Menu m: m.MainPage = item.IsActive; break;
                }
            }
            else if (checkbox.Equals("ImageState", StringComparison.OrdinalIgnoreCase))
            {
                if (entity is BaseContent content) content.ImageState = item.IsActive;
            }
            else if (checkbox.Equals("IsCampaign", StringComparison.OrdinalIgnoreCase))
            {
                if (entity is Product product) product.IsCampaign = item.IsActive;
            }

            entity.UpdatedDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
