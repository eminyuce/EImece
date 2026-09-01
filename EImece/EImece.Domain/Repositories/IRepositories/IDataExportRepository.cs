using EImece.Domain.GenericRepository;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    /// <summary>
    /// Read-only, paged data access for the full-application data export.
    /// </summary>
    public interface IDataExportRepository
    {
        Task<List<T>> GetPageAsync<T>(int skip, int take, CancellationToken cancellationToken) where T : class, IEntity<int>;

        Task<Dictionary<string, int>> GetEntityCountsAsync(CancellationToken cancellationToken);
    }
}
