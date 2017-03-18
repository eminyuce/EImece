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
using EImece.Domain.Helpers;
using System.Data.Entity.Validation;
using EImece.Domain.Models.FrontModels;
using GenericRepository.EntityFramework.Enums;

namespace EImece.Domain.Services
{
    public class ProductCategoryService : BaseContentService<ProductCategory>, IProductCategoryService
    {
        protected static readonly Logger ProductCategoryServiceLogger = LogManager.GetCurrentClassLogger();


        [Inject]
        public IProductService ProductService { get; set; }

        private IProductCategoryRepository ProductCategoryRepository { get; set; }
        public ProductCategoryService(IProductCategoryRepository repository) : base(repository)
        {
            ProductCategoryRepository = repository;
        }

        public List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1)
        {
            var cacheKey = String.Format("ProductCategoryTree-{0}-{1}", isActive, language);
            List<ProductCategoryTreeModel> result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = ProductCategoryRepository.BuildTree(isActive, language);
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);
            }
            return result;
        }

        public List<Node> CreateProductCategoryTreeViewDataList(int language)
        {
            return ProductCategoryRepository.CreateProductCategoryTreeViewDataList(language);
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

        public List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language)
        {
            return ProductCategoryRepository.GetProductCategoryLeaves(isActive, language);
        }
        public void DeleteProductCategories(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    DeleteProductCategory(id);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                ProductCategoryServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                ProductCategoryServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }
        public void DeleteProductCategory(int productCategoryId)
        {
            var productCategory = ProductCategoryRepository.GetProductCategory(productCategoryId);
            var leaves = GetProductCategoryLeaves(null, productCategory.Lang);
            if (leaves.Any(r => r.Id == productCategoryId))
            {
                if (productCategory.MainImageId.HasValue)
                {
                    FileStorageService.DeleteFileStorage(productCategory.MainImageId.Value);
                }

                var productIdList = productCategory.Products.Select(r => r.Id).ToList();
                foreach (var id in productIdList)
                {
                    ProductService.DeleteProductById(id);
                }


                DeleteEntity(productCategory);
            }

        }

        public List<ProductCategory> GetMainPageProductCategories(int language)
        {
            return ProductCategoryRepository.GetMainPageProductCategories(language);
        }

        public List<ProductCategory> GetAdminProductCategories(string search, int language)
        {
            return ProductCategoryRepository.GetAdminProductCategories(search, language);
        }

        public List<ProductCategoryTreeModel> GetBreadCrumb(int productCategoryId, int language)
        {
            var cacheKey = String.Format("ProductCategoryGetBreadCrumb-{0}-{1}", productCategoryId,language);
            List<ProductCategoryTreeModel> result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new List<ProductCategoryTreeModel>();
                var tree = BuildTree(true, language);
                ProductCategoryTreeModel productCategoryTreeModel = null;
                foreach (var t in tree)
                {
                    productCategoryTreeModel = FindNode(t, productCategoryId);
                    if (productCategoryTreeModel != null)
                    {
                        break;
                    }
                }

                AddParent(result, productCategoryTreeModel);

                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);

            }
            return result;
        }
        private void AddParent(List<ProductCategoryTreeModel> returnList, ProductCategoryTreeModel leave)
        {
            if (leave != null && leave.ProductCategory != null)
            {
                returnList.Add(leave);
            }
            if (leave != null && leave.ProductCategory != null && leave.ProductCategory.Parent != null)
            {
                AddParent(returnList, leave.Parent);
            }
        }
        private ProductCategoryTreeModel FindNode(ProductCategoryTreeModel rootNode, int Id)
        {
            if (rootNode.ProductCategory.Id == Id) return rootNode;
            if(rootNode.Childrens!=null && rootNode.Childrens.Any())
            {
                foreach (var child in rootNode.Childrens)
                {
                    var n = FindNode(child, Id);
                    if (n != null) return n;
                }
            }
           
            return null;
        }

        public ProductCategoryViewModel GetProductCategoryViewModel(int categoryId)
        {
            var cacheKey = String.Format("GetProductCategoryViewModel-{0}", categoryId);
            ProductCategoryViewModel result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new ProductCategoryViewModel();
                result.ProductCategory = GetProductCategory(categoryId);
                var tree = BuildTree(true, result.ProductCategory.Lang);

                result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, result.ProductCategory.Lang).FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
                result.ProductMenu = MenuService.GetActiveBaseContentsFromCache(true, result.ProductCategory.Lang).FirstOrDefault(r1 => r1.MenuLink.Equals("products-index", StringComparison.InvariantCultureIgnoreCase));

                result.ProductCategoryTree = tree;
                result.ChildrenProductCategories = ProductCategoryRepository.GetProductCategoriesByParentId(categoryId);
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);

            }
            return result;
        }
    }
}
