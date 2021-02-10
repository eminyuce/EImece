using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using GenericRepository;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductTagRepository : IBaseRepository<ProductTag>
    {
        List<ProductTag> GetAllByProductId(int productId);

        void SaveProductTags(int id, int[] tags);

        void DeleteProductTags(int productId);

        PaginatedList<ProductTag> GetProductsByTagId(int tagId, int pageIndex, int pageSize, int lang);

        PaginatedList<ProductTag> GetProductsByTagId(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting);
    }
}