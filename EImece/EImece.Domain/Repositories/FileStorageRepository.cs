using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.GenericRepositories;

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
