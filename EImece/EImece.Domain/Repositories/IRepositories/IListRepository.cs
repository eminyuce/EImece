﻿using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IListRepository : IBaseEntityRepository<List>, IDisposable
    {
        List GetListById(int id);
        List GetListByName(string name);
    }
}