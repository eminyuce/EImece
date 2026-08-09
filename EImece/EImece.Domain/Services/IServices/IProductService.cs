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

        Task<List<Product>> GetAdminPageListAsync(int id, string search, int lang);

        Task<List<Product>> GetAdminPageListAsync(int id, int brandId, string search, int lang);

        Task<List<Product>> GetAdminPageListAsync(int id, int brandId, string search, int lang, ProductAdminListFilter filter);

        Rss20FeedFormatter GetProductsRss(RssParams rssParams);

        ProductIndexViewModel GetMainPageProducts(int pageIndex, int lang);

        Task<ProductIndexViewModel> GetMainPageProductsAsync(int pageIndex, int lang, CancellationToken cancellationToken = default(CancellationToken));

        List<ProductTag> GetProductTagsByProductId(int productId);

        void SaveProductTags(int id, int[] tags);

        ProductAdminModel GetProductAdminPage(int categoryId, String search, int lang, int productId);

        ProductDetailViewModel GetProductDetailViewModelById(int id);

        Product GetProductById(int id);

        ProductDeleteResult DeleteProductById(int id);

        new void DeleteBaseEntity(List<string> values);

        ProductsSearchViewModel SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting);

        Task<ProductsSearchViewModel> SearchProductsAsync(int pageIndex, int pageSize, string search, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken));

        SimiliarProductTagsViewModel GetProductByTagId(int tagId, int pageIndex, int pageSize, int lang);

        void SaveProductSpecifications(List<ProductSpecification> specifications, int productId);

        String UpdatePrices(UpdatePriceRequest request);

        List<Product> GetActiveProducts(int? language);

        ProductsSearchResult GetProductsSearchResult(
         string search,
         string filters,
         string page,
         int language);

        void ParseTemplateAndSaveProductSpecifications(int productId, int templateId, int currentLanguage, HttpRequestBase request);

        void MoveProductsInTrees(int newCategoryId, string products);

        List<Product> GetChildrenProducts(ProductCategory productCategory, List<ProductCategory> ChildrenProductCategories);

        SimiliarProductTagsViewModel GetProductByTagId(int tagId, int page, int pageSize, int currentLanguage, SortingType sorting);
        void ChangeProductState(List<string> values, ProductState state);
    }
}