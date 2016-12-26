using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;

namespace EImece.Domain.Services
{

    public class FileStorageService : BaseEntityService<FileStorage>, IFileStorageService
    {
        public IFileStorageRepository FileStorageRepository { get; set; }
        public FileStorageService(IFileStorageRepository  repository) : base(repository)
        {
            FileStorageRepository = repository;
        }
    }
}
