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

namespace EImece.Domain.Services
{
    public class ProductService : BaseContentService<Product>, IProductService
    {
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

        public List<Product> GetMainPageProducts(int page, int lang)
        {
            return ProductRepository.GetMainPageProducts(page, lang);
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
                result.Product = new Product();
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
    }
}
