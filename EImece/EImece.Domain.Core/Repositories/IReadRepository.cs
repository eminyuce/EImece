using System.Linq.Expressions;
using EImece.Domain.Core.Entities;

namespace EImece.Domain.Core.Repositories;

/// <summary>
/// Thin read-side repository for Phase 3. Full repository port continues in later phases.
/// </summary>
public interface IReadRepository<TEntity> where TEntity : class, IEntity<int>
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        int take = 100,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}
