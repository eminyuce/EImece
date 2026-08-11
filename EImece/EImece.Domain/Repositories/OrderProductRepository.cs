using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Repositories
{
    public class OrderProductRepository : BaseRepository<OrderProduct>, IOrderProductRepository
    {
        public OrderProductRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public Dictionary<int, int> GetSoldQuantities(IEnumerable<int> productIds)
        {
            var ids = productIds == null ? new List<int>() : productIds.Where(id => id > 0).Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<int, int>();
            }

            return GetAll()
                .Where(op => ids.Contains(op.ProductId))
                .GroupBy(op => op.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionary(x => x.ProductId, x => x.Quantity);
        }
    }
}