using EImece.Domain.Entities;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IFileStorageRepository : IBaseEntityRepository<FileStorage>
    {
        FileStorage GetFileStoragebyFileName(string fileName);
    }
}