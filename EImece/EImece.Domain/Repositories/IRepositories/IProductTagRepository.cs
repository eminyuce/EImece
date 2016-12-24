using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductTagRepository : IBaseRepository<ProductTag>, IDisposable
    {
        List<ProductTag> GetAllByProductId(int productId);
        void SaveProductTags(int id, int[] tags);
        void DeleteProductTags(int productId);
    }
}
