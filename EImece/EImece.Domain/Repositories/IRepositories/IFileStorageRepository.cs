using EImece.Domain.Entities;
using System;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IFileStorageRepository : IBaseEntityRepository<FileStorage>
    {
        FileStorage GetFileStoragebyFileName(string fileName);
    }
}