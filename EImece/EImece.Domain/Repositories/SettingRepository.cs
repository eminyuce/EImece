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
    public class SettingRepository : BaseEntityRepository<Setting>, ISettingRepository
    {
        public SettingRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }
         
       
    }
}