using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Linq;

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
    }
}