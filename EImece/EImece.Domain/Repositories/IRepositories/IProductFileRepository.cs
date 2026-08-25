using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductFileRepository : IBaseEntityRepository<ProductFile>
    {
        Task<List<ProductFile>> GetProductFilesForImageExportAsync(CancellationToken cancellationToken = default(CancellationToken));
        List<ProductFile> GetProductFilesForImageExport();
    }
}