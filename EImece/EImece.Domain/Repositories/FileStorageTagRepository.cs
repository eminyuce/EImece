using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;

namespace EImece.Domain.Repositories
{
    public class FileStorageTagRepository : BaseRepository<FileStorageTag>, IFileStorageTagRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public FileStorageTagRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}