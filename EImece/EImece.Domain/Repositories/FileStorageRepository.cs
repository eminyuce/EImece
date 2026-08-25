using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class FileStorageRepository : BaseEntityRepository<FileStorage>, IFileStorageRepository
    {
        public FileStorageRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public FileStorage GetFileStoragebyFileName(string fileName)
        {
            return GetAll().FirstOrDefault(r => r.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task<List<FileStorage>> GetAllForImageExportAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.FileStorages.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public List<FileStorage> GetAllForImageExport()
        {
            return EImeceDbContext.FileStorages.AsNoTracking().ToList();
        }
    }
}