﻿using EImece.Domain.Entities;
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
    public class StoryCategoryRepository : BaseRepository<StoryCategory, int>, IStoryCategoryRepository
    {
        public StoryCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public int DeleteItem(StoryCategory item)
        {
            return BaseEntityRepository.DeleteItem(this, item);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int SaveOrEdit(StoryCategory item)
        {
            return BaseEntityRepository.SaveOrEdit(this, item);
        }
    }
}