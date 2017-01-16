using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharkDev.Web.Controls.TreeView.Model;
using NLog;

namespace EImece.Domain.Services
{
    public class ProductCategoryService : BaseContentService<ProductCategory>, IProductCategoryService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IProductCategoryRepository ProductCategoryRepository { get; set; }
        public ProductCategoryService(IProductCategoryRepository repository) : base(repository)
        {
            ProductCategoryRepository = repository;
        }

        public List<ProductCategory> BuildTree(bool? isActive, int language = 1)
        {
            var cacheKey = String.Format("ProductCategoryTree-{0}-{1}", isActive, language);
            List<ProductCategory> result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = ProductCategoryRepository.BuildTree(isActive, language);
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);
            }
            return result;
        }

        public List<Node> CreateProductCategoryTreeViewDataList()
        {
            return ProductCategoryRepository.CreateProductCategoryTreeViewDataList();
        }

        public ProductCategory GetProductCategory(int categoryId)
        {
            var cacheKey = String.Format("ProductCategory-{0}", categoryId);
            ProductCategory result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = ProductCategoryRepository.GetProductCategory(categoryId);
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);
 
            }
            return result;

        }
    }
}
