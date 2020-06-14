using EImece.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface IListService : IBaseEntityService<List>
    {
        void DeleteListById(int id);

        List GetListById(int id);

        List GetListByName(String name);

        List<List> GetListItems();
    }
}