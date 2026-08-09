using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductRepository : IBaseContentRepository<Product>
    {
        PaginatedList<Product> GetActiveProducts(int pageIndex, int pageSize, int language);

        Task<PaginatedList<Product>> GetActiveProductsAsync(int pageIndex, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken));

        PaginatedList<Product> GetMainPageProducts(int pageIndex, int pageSize, int language);

        List<Product> GetAdminPageList(int categoryId, string search, int language);

        List<Product> GetAdminPageList(int categoryId, int brandId, string search, int language);

        List<Product> GetAdminPageList(int categoryId, int brandId, string search, int language, ProductAdminListFilter filter);

        Task<List<Product>> GetAdminPageListAsync(int categoryId, string search, int language);

        Task<List<Product>> GetAdminPageListAsync(int categoryId, int brandId, string search, int language);

        Task<List<Product>> GetAdminPageListAsync(int categoryId, int brandId, string search, int language, ProductAdminListFilter filter);

        Product GetProduct(int id);

        Task<Product> GetProductAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        PaginatedList<Product> SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting);

        Task<PaginatedList<Product>> SearchProductsAsync(int pageIndex, int pageSize, string search, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken));

        IEnumerable<Product> GetData(out int totalRecords, string globalSearch, String name, int? limitOffset, int? limitRowCount, string orderBy, bool desc);

        List<Product> GetRelatedProducts(int[] tagIdList, int take, int lang, int excludedProductId);

        Task<List<Product>> GetRelatedProductsAsync(int[] tagIdList, int take, int lang, int excludedProductId, CancellationToken cancellationToken = default(CancellationToken));

        List<Product> GetActiveProducts(int? language);

        Task<List<Product>> GetActiveProductsAsync(int? language, CancellationToken cancellationToken = default(CancellationToken));

        ProductsSearchResult GetProductsSearchResult(
   string search,
   string filters,
   int top,
   int skip,
   int language);

        Task<ProductsSearchResult> GetProductsSearchResultAsync(
   string search,
   string filters,
   int top,
   int skip,
   int language,
   CancellationToken cancellationToken = default(CancellationToken));

        List<Product> GetChildrenProducts(int[] childrenCategoryId);

        Task<List<Product>> GetChildrenProductsAsync(int[] childrenCategoryId, CancellationToken cancellationToken = default(CancellationToken));

        List<Product> GetRandomProductsByCategoryId(int productCategoryId, int take, int lang, int excludedProductId);

        Task<List<Product>> GetRandomProductsByCategoryIdAsync(int productCategoryId, int take, int lang, int excludedProductId, CancellationToken cancellationToken = default(CancellationToken));
    }
}