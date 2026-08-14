using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.DTOs.Storefront;
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

        List<Product> GetActiveProducts(int? language);

        Task<PaginatedList<Product>> GetActiveProductsAsync(int pageIndex, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<Product>> GetActiveProductsAsync(int? language, CancellationToken cancellationToken = default(CancellationToken));

        PaginatedList<Product> GetMainPageProducts(int pageIndex, int pageSize, int language);

        List<Product> GetAdminPageList(int categoryId, string search, int language);

        List<Product> GetAdminPageList(int categoryId, int brandId, string search, int language);

        List<Product> GetAdminPageList(int categoryId, int brandId, string search, int language, ProductAdminListFilter filter);

        Task<List<Product>> GetAdminPageListAsync(int categoryId, string search, int language, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<Product>> GetAdminPageListAsync(int categoryId, int brandId, string search, int language, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<Product>> GetAdminPageListAsync(int categoryId, int brandId, string search, int language, ProductAdminListFilter filter, CancellationToken cancellationToken = default(CancellationToken));

        Product GetProduct(int id);

        Task<Product> GetProductAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        PaginatedList<Product> SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting);

        Task<PaginatedList<Product>> SearchProductsAsync(int pageIndex, int pageSize, string search, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken));

        IEnumerable<Product> GetData(out int totalRecords, string globalSearch, String name, int? limitOffset, int? limitRowCount, string orderBy, bool desc);

        List<Product> GetRelatedProducts(int[] tagIdList, int take, int lang, int excludedProductId);

        Task<List<Product>> GetRelatedProductsAsync(int[] tagIdList, int take, int lang, int excludedProductId, CancellationToken cancellationToken = default(CancellationToken));

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

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Minimal Columns)

        Task<StorefrontProductCardDto> GetStorefrontProductCardByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        StorefrontProductCardDto GetStorefrontProductCardById(int id);

        Task<StorefrontProductDetailDto> GetStorefrontProductDetailByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        StorefrontProductDetailDto GetStorefrontProductDetailById(int id);

        Task<List<StorefrontProductCardDto>> GetStorefrontMainPageProductsAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontProductCardDto> GetStorefrontMainPageProducts(int take, int language);

        Task<List<StorefrontProductCardDto>> GetStorefrontLatestProductsAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontProductCardDto> GetStorefrontLatestProducts(int take, int language);

        Task<List<StorefrontProductCardDto>> GetStorefrontCampaignProductsAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontProductCardDto> GetStorefrontCampaignProducts(int take, int language);

        Task<List<StorefrontProductCardDto>> GetStorefrontActiveProductsAsync(int? language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontProductCardDto> GetStorefrontActiveProducts(int? language);

        Task<PaginatedList<StorefrontProductCardDto>> GetStorefrontActiveProductsPagedAsync(int pageIndex, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken));

        PaginatedList<StorefrontProductCardDto> GetStorefrontActiveProductsPaged(int pageIndex, int pageSize, int language);

        Task<List<StorefrontProductCardDto>> GetStorefrontCategoryProductsAsync(int categoryId, int language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontProductCardDto> GetStorefrontCategoryProducts(int categoryId, int language);

        Task<PaginatedList<StorefrontProductCardDto>> GetStorefrontProductsByCategoryIdAsync(int categoryId, List<int> childCategoryIds, int language, int pageIndex, int pageSize, SortingType sorting, decimal? minPrice, decimal? maxPrice, List<int> brandIds, List<int> ratings, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<StorefrontProductCardDto>> GetStorefrontRelatedProductsAsync(int[] tagIdList, int take, int language, int excludedProductId, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontProductCardDto> GetStorefrontRelatedProducts(int[] tagIdList, int take, int language, int excludedProductId);

        Task<List<StorefrontProductCardDto>> GetStorefrontRandomProductsByCategoryIdAsync(int productCategoryId, int take, int language, int excludedProductId, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontProductCardDto> GetStorefrontRandomProductsByCategoryId(int productCategoryId, int take, int language, int excludedProductId);

        Task<PaginatedList<StorefrontProductCardDto>> SearchStorefrontProductsAsync(int pageIndex, int pageSize, string search, int language, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken));

        PaginatedList<StorefrontProductCardDto> SearchStorefrontProducts(int pageIndex, int pageSize, string search, int language, SortingType sorting);

        Task<PaginatedList<StorefrontProductCardDto>> GetStorefrontProductsByTagIdAsync(int tagId, int pageIndex, int pageSize, int language, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken));

        PaginatedList<StorefrontProductCardDto> GetStorefrontProductsByTagId(int tagId, int pageIndex, int pageSize, int language, SortingType sorting);

        #endregion
    }
}