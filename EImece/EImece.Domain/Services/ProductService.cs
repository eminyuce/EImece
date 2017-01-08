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

       

        private IProductRepository ProductRepository { get; set; }
        public ProductService(IProductRepository repository) : base(repository)
        {
            ProductRepository = repository;
        }

        public List<Product> GetAdminPageList(int categoryId, string search, int lang)
        {
            return ProductRepository.GetAdminPageList(categoryId, search, lang);
        }

        public List<Product> GetMainPageProducts(int pageIndex, int pageSize, int lang)
        {
            return ProductRepository.GetMainPageProducts(pageIndex,pageSize, lang);
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
            result.ProductCategoryTree = ProductCategoryService.CreateProductCategoryTreeViewDataList();
            if (productId > 0)
            {
                result.Product = this.GetProductById(productId);
            }
            else
            {
                result.Product = Product.GetInstance<Product>();
                if (categoryId > 0)
                {
                    result.Product.ProductCategoryId = categoryId;
                    result.Product.ProductCategory = ProductCategoryService.GetSingle(categoryId);
                }
            }
            EImeceLanguage language = (EImeceLanguage)lang;
            result.TagCategories = TagCategoryService.GetTagsByTagType(EImeceTagType.Products, language);


            return result;
        }

        public Product GetProductById(int id)
        {
            return ProductRepository.GetProduct(id);
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
            var product = GetProductById(id);
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

       
    }
}
