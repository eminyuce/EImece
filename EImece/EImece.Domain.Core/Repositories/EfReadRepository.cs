using System.Linq.Expressions;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EImece.Domain.Core.Repositories;

public class EfReadRepository<TEntity> : IReadRepository<TEntity>
    where TEntity : class, IEntity<int>
{
    private readonly EImeceDbContext _db;

    public EfReadRepository(EImeceDbContext db)
    {
        _db = db;
    }

    public Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _db.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _db.Set<TEntity>().AsNoTracking();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.Take(take).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _db.Set<TEntity>().AsNoTracking();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return query.CountAsync(cancellationToken);
    }
}
