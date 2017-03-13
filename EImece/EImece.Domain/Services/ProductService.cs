using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using Ninject;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using System.Data.Entity.Validation;
using EImece.Domain.Helpers;
using NLog;
using EImece.Domain.Models.FrontModels;

namespace EImece.Domain.Services
{
    public class ProductService : BaseContentService<Product>, IProductService
    {
        private static readonly Logger ProductServiceLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }
        [Inject]
        public ITagService TagService { get; set; }
        [Inject]
        public IStoryService StoryService { get; set; }
        [Inject]
        public IStoryRepository StoryRepository { get; set; }
        [Inject]
        public ITemplateRepository TemplateRepository { get; set; }

        private IProductRepository ProductRepository { get; set; }
        public ProductService(IProductRepository repository) : base(repository)
        {
            ProductRepository = repository;
        }

        public List<Product> GetAdminPageList(int categoryId, string search, int lang)
        {
            return ProductRepository.GetAdminPageList(categoryId, search, lang);
        }

        public ProductIndexViewModel GetMainPageProducts(int page, int language)
        {
            ProductIndexViewModel result = null;
            var cacheKey = String.Format("GetMainPageProducts-{0}-{1}", page, language);

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new ProductIndexViewModel();
                int pageSize = Settings.RecordPerPage;
                result.CompanyName = SettingService.GetSettingObjectByKey(Settings.CompanyName);
                var items = ProductRepository.GetMainPageProducts(page, pageSize, language);
                result.Products = items;
                result.Tags = TagService.GetActiveBaseEntities(true, language);
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);

            }
            return result;
        }


        public void SaveProductTags(int id, int[] tags)
        {
            ProductTagRepository.SaveProductTags(id, tags);
        }

        public List<ProductTag> GetProductTagsByProductId(int productId)
        {
            return ProductTagRepository.GetAllByProductId(productId);
        }

        public ProductAdminModel GetProductAdminPage(int categoryId, String search, int lang, int productId)
        {
            var result = new ProductAdminModel();
            result.Products = this.GetAdminPageList(categoryId, search, lang);
            result.ProductCategoryTree = ProductCategoryService.CreateProductCategoryTreeViewDataList(lang);
            if (productId > 0)
            {
                result.Product = ProductRepository.GetProduct(productId);
            }
            else
            {
                result.Product = EntityFactory.GetBaseContentInstance<Product>();
                if (categoryId > 0)
                {
                    result.Product.ProductCategoryId = categoryId;
                    result.Product.ProductCategory = ProductCategoryService.GetSingle(categoryId);
                }
            }
            EImeceLanguage language = (EImeceLanguage)lang;
            result.TagCategories = TagCategoryService.GetTagsByTagType(language);


            return result;
        }

        public ProductDetailViewModel GetProductById(int id)
        {
            var cacheKey = String.Format("ProductById-{0}", id);
            ProductDetailViewModel result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new ProductDetailViewModel();
                var r = ProductRepository.GetProduct(id);
                result.Product = r;
                result.Template = TemplateRepository.GetSingle(r.ProductCategory.TemplateId.Value);
                result.BreadCrumb = ProductCategoryService.GetBreadCrumb(r.ProductCategoryId, r.Lang);
                result.RelatedStories = new List<Story>();
                if (r != null && r.ProductTags.Any())
                {
                    var tagIdList = r.ProductTags.Select(t => t.TagId).ToArray();
                    result.RelatedStories = StoryRepository.GetRelatedStories(tagIdList, 10, r.Lang, 0);
                }
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);

            }
            return result;
        }
        public virtual new void DeleteBaseEntity(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    DeleteProductById(id);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                ProductServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                ProductServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }
        public void DeleteProductById(int id)
        {
            var product = ProductRepository.GetProduct(id);
            ProductSpecificationRepository.DeleteByWhereCondition(r => r.ProductId == id);
            ProductTagRepository.DeleteByWhereCondition(r => r.ProductId == id);
            if (product.MainImageId.HasValue)
            {
                FileStorageService.DeleteFileStorage(product.MainImageId.Value);
            }
            if (product.ProductFiles != null)
            {
                foreach (var file in product.ProductFiles)
                {
                    FileStorageService.DeleteFileStorage(file.FileStorageId);
                }
                ProductFileRepository.DeleteByWhereCondition(r => r.ProductId == id);
            }
            DeleteEntity(product);

        }

        public ProductsSearchViewModel SearchProducts(int pageIndex, int pageSize, string search, int lang)
        {
            var r = new ProductsSearchViewModel();
            r.Search = search;
            if (!String.IsNullOrEmpty(search))
            {
                r.Products = ProductRepository.SearchProducts(pageIndex, pageSize, search, lang);
            }
            else
            {
                r.Products = new GenericRepository.PaginatedList<Product>(new List<Product>(), pageIndex, pageSize, 0);
            }

            return r;
        }

        public SimiliarProductTagsViewModel GetProductByTagId(int tagId, int pageIndex, int pageSize, int lang)
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = TagService.GetSingle(tagId);
            r.ProductTags = ProductTagRepository.GetProductsByTagId(tagId, pageIndex, pageSize, lang);
            r.StoryTags = StoryTagRepository.GetStoriesByTagId(tagId, 0, 10, lang);
            return r;
        }

        public void SaveProductSpecifications(List<ProductSpecification> specifications)
        {
            if (specifications.Any())
            {
                int productId = specifications.First().ProductId;
                ProductSpecificationRepository.DeleteByWhereCondition(r => r.ProductId == productId);
                foreach (var item in specifications)
                {
                    ProductSpecificationRepository.Add(item);
                }
                ProductSpecificationRepository.Save();
            }
        }
    }
}
