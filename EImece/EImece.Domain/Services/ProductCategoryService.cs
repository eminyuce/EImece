﻿using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

namespace EImece.Domain.Services
{
    public class ProductCategoryService : BaseContentService<ProductCategory>, IProductCategoryService
    {
        protected static readonly Logger ProductCategoryServiceLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IBrandService BrandService { get; set; }

        [Inject]
        public TemplateService TemplateService { get; set; }

        private IProductCategoryRepository ProductCategoryRepository { get; set; }

        public ProductCategoryService(IProductCategoryRepository repository) : base(repository)
        {
            ProductCategoryRepository = repository;
        }

        public ProductCategoryService(IProductCategoryRepository repository, bool IsCachingActivated) : base(repository)
        {
            this.IsCachingActivated = IsCachingActivated;
        }

        public List<ProductCategoryTreeModel> BuildNavigation(bool isActive, int language = 1)
        {
            List<ProductCategoryTreeModel> result;
            if (IsCachingActivated)
            {
                var cacheKey = String.Format("BuildNavigation-{0}-{1}", isActive, language);
                if (!DataCachingProvider.Get(cacheKey, out result))
                {
                    result = ProductCategoryRepository.BuildNavigation(isActive, language);
                    DataCachingProvider.Set(cacheKey, result, AppConfig.CacheMediumSeconds);
                }
            }
            else
            {
                result = ProductCategoryRepository.BuildTree(isActive, language);
            }

            return result;
        }

        public List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1)
        {
            List<ProductCategoryTreeModel> result;
            if (IsCachingActivated)
            {
                var cacheKey = String.Format("ProductCategoryTree-{0}-{1}", isActive, language);
                if (!DataCachingProvider.Get(cacheKey, out result))
                {
                    result = ProductCategoryRepository.BuildTree(isActive, language);
                    DataCachingProvider.Set(cacheKey, result, AppConfig.CacheMediumSeconds);
                }
            }
            else
            {
                result = ProductCategoryRepository.BuildTree(isActive, language);
            }

            return result;
        }

        public ProductCategory GetProductCategory(int categoryId)
        {
            ProductCategory result = ProductCategoryRepository.GetProductCategory(categoryId);
            return EntityFilterHelper.FilterProductCategory(result);
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
            var productCategory = ProductCategoryRepository.GetProductCategory(productCategoryId, false);
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
            List<ProductCategory> result;
            var cacheKey = $"GetMainPageProductCategories-{language}";
            if (!DataCachingProvider.Get(cacheKey, out result))
            {
                result = ProductCategoryRepository.GetMainPageProductCategories(language);
                DataCachingProvider.Set(cacheKey, result, AppConfig.CacheLongSeconds);
            }

            return result;
        }

        public List<ProductCategory> GetAdminProductCategories(string search, int currentLanguage)
        {
            return ProductCategoryRepository.GetAdminProductCategories(search, currentLanguage);
        }

        public List<ProductCategoryTreeModel> GetBreadCrumb(int productCategoryId, int language)
        {
            List<ProductCategoryTreeModel> result = new List<ProductCategoryTreeModel>();

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
            if (rootNode.Childrens != null && rootNode.Childrens.Any())
            {
                foreach (var child in rootNode.Childrens)
                {
                    var n = FindNode(child, Id);
                    if (n != null) return n;
                }
            }

            return null;
        }

        public ProductCategoryViewModel GetProductCategoryViewModel(int productCategoryId)
        {
            var result = new ProductCategoryViewModel();
            result.ProductCategory = GetProductCategory(productCategoryId);
            if (result.ProductCategory.ParentId > 0)
            {
                result.ProductCategory.Parent = GetProductCategory(result.ProductCategory.ParentId);
                if (result.ProductCategory.Parent.ParentId > 0)
                {
                    result.ProductCategory.Parent.Parent = GetProductCategory(result.ProductCategory.Parent.ParentId);
                }
            }
            int lang = result.ProductCategory.Lang;
            List<Menu> lists = MenuService.GetActiveBaseContentsFromCache(true, lang);
            result.MainPageMenu = lists.FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            result.ProductMenu = lists.FirstOrDefault(r1 => r1.MenuLink.Equals("products-index", StringComparison.InvariantCultureIgnoreCase));
            result.Brands = BrandService.GetBrandsIfAnyProductExists(lang);
            result.ProductCategoryTree = BuildTree(true, lang);
            result.PriceFilterSetting = SettingService.GetSettingObjectByKey(Constants.ProductPriceFilterSetting);
            result.ChildrenProductCategories = ProductCategoryRepository.GetProductCategoriesByParentId(productCategoryId);
            result.CategoryChildrenProducts = ProductService.GetChildrenProducts(result.ProductCategory, result.ChildrenProductCategories);
            return result;
        }
    }
}