using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;

namespace EImece.Domain.Services.IServices
{
    public interface IProductService : IBaseContentService<Product>
    {
        List<Product> GetAdminPageList(int id, string search, int lang);

        Rss20FeedFormatter GetProductsRss(RssParams rssParams);

        ProductIndexViewModel GetMainPageProducts(int pageIndex, int lang);

        List<ProductTag> GetProductTagsByProductId(int productId);

        void SaveProductTags(int id, int[] tags);

        ProductAdminModel GetProductAdminPage(int categoryId, String search, int lang, int productId);

        ProductDetailViewModel GetProductDetailViewModelById(int id);

        Product GetProductById(int id);

        void DeleteProductById(int id);

        new void DeleteBaseEntity(List<string> values);

        ProductsSearchViewModel SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting);

        SimiliarProductTagsViewModel GetProductByTagId(int tagId, int pageIndex, int pageSize, int lang);

        void SaveProductSpecifications(List<ProductSpecification> specifications, int productId);

        List<Product> GetActiveProducts(bool? isActive, int? language);

        ProductsSearchResult GetProductsSearchResult(
         string search,
         string filters,
         string page,
         int language);

        void ParseTemplateAndSaveProductSpecifications(int productId, int templateId, int currentLanguage, HttpRequestBase request);

        void MoveProductsInTrees(int newCategoryId, string products);

        List<Product> GetChildrenProducts(ProductCategory productCategory, List<ProductCategory> ChildrenProductCategories);
    }
}