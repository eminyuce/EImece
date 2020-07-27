using EImece.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IListRepository : IBaseEntityRepository<List>
    {
        List GetListById(int id);

        List GetListByName(string name);

        List<List> GetAllListItems();
    }
}