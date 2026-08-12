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

        Task<List<ProductTag>> GetAllByProductIdAsync(int productId, CancellationToken cancellationToken = default(CancellationToken));

        void SaveProductTags(int id, int[] tags);

        Task SaveProductTagsAsync(int id, int[] tags);

        void DeleteProductTags(int productId);

        Task DeleteProductTagsAsync(int productId);

        PaginatedList<ProductTag> GetProductsByTagId(int tagId, int pageIndex, int pageSize, int lang);

        PaginatedList<ProductTag> GetProductsByTagId(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting);

        Task<PaginatedList<ProductTag>> GetProductsByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, CancellationToken cancellationToken = default(CancellationToken));

        Task<PaginatedList<ProductTag>> GetProductsByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken));
    }
}