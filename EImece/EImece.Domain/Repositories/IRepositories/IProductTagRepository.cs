using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Models.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductTagRepository : IBaseRepository<ProductTag>
    {
        List<ProductTag> GetAllByProductId(int productId);

        void SaveProductTags(int id, int[] tags);

        void DeleteProductTags(int productId);

        PaginatedList<ProductTag> GetProductsByTagId(int tagId, int pageIndex, int pageSize, int lang);

        Task<PaginatedList<ProductTag>> GetProductsByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, CancellationToken cancellationToken = default(CancellationToken));

        PaginatedList<ProductTag> GetProductsByTagId(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting);

        Task<PaginatedList<ProductTag>> GetProductsByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken));
    }
}