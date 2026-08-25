using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IFileStorageRepository : IBaseEntityRepository<FileStorage>
    {
        FileStorage GetFileStoragebyFileName(string fileName);

        Task<List<FileStorage>> GetAllForImageExportAsync(CancellationToken cancellationToken = default(CancellationToken));

        List<FileStorage> GetAllForImageExport();
    }
}