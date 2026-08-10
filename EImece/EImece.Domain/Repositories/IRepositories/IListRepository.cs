using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IListRepository : IBaseEntityRepository<List>
    {
        List GetListById(int id);

        Task<List> GetListByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        List GetListByName(string name);

        List<List> GetAllListItems();
    }
}