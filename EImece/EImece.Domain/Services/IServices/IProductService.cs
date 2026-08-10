using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace EImece.Domain.Services.IServices
{
    public interface IProductService : IBaseContentService<Product>
    {
        List<Product> GetAdminPageList(int id, string search, int lang);

        List<Product> GetAdminPageList(int id, int brandId, string search, int lang);

        List<Product> GetAdminPageList(int id, int brandId, string search, int lang, ProductAdminListFilter filter);

        Task<List<Product>> GetAdminPageListAsync(int id, string search, int lang, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<Product>> GetAdminPageListAsync(int id, int brandId, string search, int lang, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<Product>> GetAdminPageListAsync(int id, int brandId, string search, int lang, ProductAdminListFilter filter, CancellationToken cancellationToken = default(CancellationToken));

        Rss20FeedFormatter GetProductsRss(RssParams rssParams);

        Task<Rss20FeedFormatter> GetProductsRssAsync(RssParams rssParams, CancellationToken cancellationToken = default(CancellationToken));

        ProductIndexViewModel GetMainPageProducts(int pageIndex, int lang);

        Task<ProductIndexViewModel> GetMainPageProductsAsync(int pageIndex, int lang, CancellationToken cancellationToken = default(CancellationToken));

        List<ProductTag> GetProductTagsByProductId(int productId);

        void SaveProductTags(int id, int[] tags);

        Task SaveProductTagsAsync(int id, int[] tags);

        ProductAdminModel GetProductAdminPage(int categoryId, String search, int lang, int productId);

        ProductDetailViewModel GetProductDetailViewModelById(int id);

        Task<ProductDetailViewModel> GetProductDetailViewModelByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        Product GetProductById(int id);

        Task<Product> GetProductByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        ProductDeleteResult DeleteProductById(int id);

        Task<ProductDeleteResult> DeleteProductByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        new void DeleteBaseEntity(List<string> values);

        ProductsSearchViewModel SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting);

        Task<ProductsSearchViewModel> SearchProductsAsync(int pageIndex, int pageSize, string search, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken));

        SimiliarProductTagsViewModel GetProductByTagId(int tagId, int pageIndex, int pageSize, int lang);

        Task<SimiliarProductTagsViewModel> GetProductByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, CancellationToken cancellationToken = default(CancellationToken));

        void SaveProductSpecifications(List<ProductSpecification> specifications, int productId);

        Task SaveProductSpecificationsAsync(List<ProductSpecification> specifications, int productId);

        String UpdatePrices(UpdatePriceRequest request);

        List<Product> GetActiveProducts(int? language);

        Task<List<Product>> GetActiveProductsAsync(int? language, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops every <c>product:list:*</c> / <c>product:search:*</c> MemoryCache entry.
        /// Called from product mutations and from the Admin Refresh button before <c>ClearAll</c>.
        /// </summary>
        void InvalidateProductListCaches();

        ProductsSearchResult GetProductsSearchResult(
         string search,
         string filters,
         string page,
         int language);

        Task<ProductsSearchResult> GetProductsSearchResultAsync(
         string search,
         string filters,
         string page,
         int language,
         CancellationToken cancellationToken = default(CancellationToken));

        void ParseTemplateAndSaveProductSpecifications(int productId, int templateId, int currentLanguage, HttpRequestBase request);

        Task ParseTemplateAndSaveProductSpecificationsAsync(int productId, int templateId, int currentLanguage, HttpRequestBase request, CancellationToken cancellationToken = default(CancellationToken));

        void MoveProductsInTrees(int newCategoryId, string products);

        Task MoveProductsInTreesAsync(int newCategoryId, string products, CancellationToken cancellationToken = default(CancellationToken));

        List<Product> GetChildrenProducts(ProductCategory productCategory, List<ProductCategory> ChildrenProductCategories);

        Task<List<Product>> GetChildrenProductsAsync(ProductCategory productCategory, List<ProductCategory> ChildrenProductCategories, CancellationToken cancellationToken = default(CancellationToken));

        SimiliarProductTagsViewModel GetProductByTagId(int tagId, int page, int pageSize, int currentLanguage, SortingType sorting);

        Task<SimiliarProductTagsViewModel> GetProductByTagIdAsync(int tagId, int page, int pageSize, int currentLanguage, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken));
        void ChangeProductState(List<string> values, ProductState state);
    }
}