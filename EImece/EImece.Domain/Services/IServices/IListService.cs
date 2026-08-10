using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IListService : IBaseEntityService<List>
    {
        void DeleteListById(int id);

        Task DeleteListByIdAsync(int id);

        List GetListById(int id);

        Task<List> GetListByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        List GetListByName(String name);

        List<List> GetListItems();
    }
}