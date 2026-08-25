using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IMenuFileRepository : IBaseEntityRepository<MenuFile>
    {
        Task<List<MenuFile>> GetMenuFilesForImageExportAsync(CancellationToken cancellationToken = default(CancellationToken));
        List<MenuFile> GetMenuFilesForImageExport();
    }
}