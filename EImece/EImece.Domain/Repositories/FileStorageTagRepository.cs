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
    public class FileStorageTagRepository : BaseRepository<FileStorageTag, int>, IFileStorageTagRepository
    {
        public FileStorageTagRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public int DeleteItem(FileStorageTag item)
        {
            return BaseEntityRepository.DeleteItem(this, item);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int SaveOrEdit(FileStorageTag item)
        {
            return BaseEntityRepository.SaveOrEdit(this, item);
        }
    }
}
