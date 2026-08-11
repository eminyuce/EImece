using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IOrderProductRepository : IBaseRepository<OrderProduct>
    {
        Dictionary<int, int> GetSoldQuantities(IEnumerable<int> productIds);
    }
}