﻿using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
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
