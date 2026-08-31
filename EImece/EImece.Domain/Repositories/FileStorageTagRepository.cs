using Microsoft.Extensions.Logging;
﻿using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
namespace EImece.Domain.Repositories
{
    public class FileStorageTagRepository : BaseRepository<FileStorageTag>, IFileStorageTagRepository
    {
        public FileStorageTagRepository(IEImeceContext dbContext, ILogger<FileStorageTagRepository> logger) : base(dbContext, logger) {
        }
    }
}