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
    public class ProductTagRepository : BaseRepository<ProductTag, int>, IProductTagRepository
    {
        public ProductTagRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public int DeleteItem(ProductTag item)
        {
            return BaseEntityRepository.DeleteItem(this, item);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int SaveOrEdit(ProductTag item)
        {
            return BaseEntityRepository.SaveOrEdit(this, item);
        }
    }
}