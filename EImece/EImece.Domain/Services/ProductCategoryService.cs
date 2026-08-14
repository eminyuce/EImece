using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        [Inject]
        public IProductRepository ProductRepository { get; set; }

        private IProductCategoryRepository ProductCategoryRepository { get; set; }

        public ProductCategoryService(IProductCategoryRepository repository) : base(repository)
        {
            ProductCategoryRepository = repository;
        }

        public ProductCategoryService(IProductCategoryRepository repository, bool IsCachingActivated) : base(repository)
        {
            this.IsCachingActivated = IsCachingActivated;
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        public async Task<StorefrontCategoryDto> GetStorefrontCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductCategoryRepository.GetStorefrontCategoryByIdAsync(categoryId, cancellationToken).ConfigureAwait(false);
        }

        public StorefrontCategoryDto GetStorefrontCategoryById(int categoryId)
        {
            return ProductCategoryRepository.GetStorefrontCategoryById(categoryId);
        }

        public async Task<List<StorefrontCategoryDto>> GetStorefrontMainPageCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = $"StorefrontMainPageCategories-{language}" + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => ProductCategoryRepository.GetStorefrontMainPageCategoriesAsync(language, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public List<StorefrontCategoryDto> GetStorefrontMainPageCategories(int language)
        {
            var cacheKey = $"StorefrontMainPageCategories-{language}";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => ProductCategoryRepository.GetStorefrontMainPageCategories(language),
                AppConfig.CacheLongSeconds);
        }

        public async Task<List<StorefrontCategoryDto>> GetStorefrontChildrenCategoriesAsync(int parentCategoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductCategoryRepository.GetStorefrontChildrenCategoriesAsync(parentCategoryId, cancellationToken).ConfigureAwait(false);
        }

        public List<StorefrontCategoryDto> GetStorefrontChildrenCategories(int parentCategoryId)
        {
            return ProductCategoryRepository.GetStorefrontChildrenCategories(parentCategoryId);
        }

        public async Task<List<StorefrontCategoryDto>> BuildStorefrontNavigationTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = $"StorefrontNavigationTree-{language}" + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => ProductCategoryRepository.BuildStorefrontNavigationTreeAsync(language, cancellationToken),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        public List<StorefrontCategoryDto> BuildStorefrontNavigationTree(int language)
        {
            var cacheKey = $"StorefrontNavigationTree-{language}";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => ProductCategoryRepository.BuildStorefrontNavigationTree(language),
                AppConfig.CacheMediumSeconds);
        }

        public async Task<ProductCategoryViewModel> GetStorefrontCategoryPageViewModelAsync(
            int categoryId,
            int page,
            EImece.Domain.Models.Enums.SortingType sorting,
            string filter,
            int? minPrice,
            int? maxPrice,
            int recordPerPage,
            int language,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var categoryDto = await ProductCategoryRepository.GetStorefrontCategoryByIdAsync(categoryId, cancellationToken).ConfigureAwait(false);
            if (categoryDto == null)
            {
                return null;
            }

            if (categoryDto.ParentId > 0)
            {
                categoryDto.Parent = await ProductCategoryRepository.GetStorefrontCategoryByIdAsync(categoryDto.ParentId, cancellationToken).ConfigureAwait(false);
                if (categoryDto.Parent != null && categoryDto.Parent.ParentId > 0)
                {
                    categoryDto.Parent.Parent = await ProductCategoryRepository.GetStorefrontCategoryByIdAsync(categoryDto.Parent.ParentId, cancellationToken).ConfigureAwait(false);
                }
            }

            var childCategories = await ProductCategoryRepository.GetStorefrontChildrenCategoriesAsync(categoryId, cancellationToken).ConfigureAwait(false);
            var childCategoryIds = childCategories.Select(c => c.Id).ToList();

            var result = new ProductCategoryViewModel();
            result.CategoryDto = categoryDto;
            result.ProductCategory = new ProductCategory
            {
                Id = categoryDto.Id,
                Name = categoryDto.Name,
                ParentId = categoryDto.ParentId,
                ShortDescription = categoryDto.ShortDescription,
                Description = categoryDto.Description,
                MetaKeywords = categoryDto.MetaKeywords,
                IsActive = categoryDto.IsActive,
                Position = categoryDto.Position,
                Lang = categoryDto.Lang,
                MainImageId = categoryDto.MainImageId,
                Parent = categoryDto.Parent != null ? new ProductCategory
                {
                    Id = categoryDto.Parent.Id,
                    Name = categoryDto.Parent.Name,
                    ParentId = categoryDto.Parent.ParentId,
                    Parent = categoryDto.Parent.Parent != null ? new ProductCategory
                    {
                        Id = categoryDto.Parent.Parent.Id,
                        Name = categoryDto.Parent.Parent.Name,
                        ParentId = categoryDto.Parent.Parent.ParentId
                    } : null
                } : null
            };

            result.ChildrenProductCategories = childCategories.Select(c => new ProductCategory
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId,
                Position = c.Position,
                Lang = c.Lang,
                IsActive = c.IsActive,
                MainImageId = c.MainImageId
            }).ToList();

            List<int> brandIds = null;
            List<int> ratings = null;
            if (!string.IsNullOrEmpty(filter))
            {
                var selectedFilters = FilterHelper.ParseFiltersFromString(filter);
                if (selectedFilters != null && selectedFilters.Any())
                {
                    brandIds = selectedFilters.Where(f => f.FieldName.Equals("BrandId", StringComparison.OrdinalIgnoreCase)).Select(f => f.ValueFirst.ToInt()).ToList();
                    ratings = selectedFilters.Where(f => f.FieldName.Equals("Rating", StringComparison.OrdinalIgnoreCase)).Select(f => f.ValueFirst.ToInt()).ToList();
                }
            }

            decimal? minP = (minPrice.HasValue && minPrice.Value > 0) ? (decimal?)minPrice.Value : null;
            decimal? maxP = (maxPrice.HasValue && maxPrice.Value > 0) ? (decimal?)maxPrice.Value : null;

            var pagedList = await ProductRepository.GetStorefrontProductsByCategoryIdAsync(
                categoryId,
                childCategoryIds,
                language,
                page > 0 ? page : 1,
                recordPerPage,
                sorting,
                minP,
                maxP,
                brandIds,
                ratings,
                cancellationToken).ConfigureAwait(false);

            result.PagedProductDtos = pagedList;
            result.Page = page > 0 ? page : 1;
            result.RecordPerPage = recordPerPage;
            result.Filter = filter;
            result.Sorting = sorting;
            result.MinPrice = minPrice;
            result.MaxPrice = maxPrice;

            result.AllProducts = new List<Product>();
            result.CategoryChildrenProducts = new List<Product>();

            List<Menu> lists = await MenuService.GetActiveBaseContentsFromCacheAsync(true, language).ConfigureAwait(false);
            result.MainPageMenu = lists.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.ProductMenu = lists.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.Brands = await BrandService.GetBrandsIfAnyProductExistsAsync(language).ConfigureAwait(false);
            result.ProductCategoryTree = await BuildTreeAsync(true, language).ConfigureAwait(false);
            result.PriceFilterSetting = await SettingService.GetSettingObjectByKeyAsync(Constants.ProductPriceFilterSetting).ConfigureAwait(false);
            result.IsProductPriceEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductPriceEnable).ConfigureAwait(false);
            result.IsProductReviewEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductReviewEnable).ConfigureAwait(false);

            return result;
        }

        #endregion

        public List<ProductCategoryTreeModel> BuildNavigation(bool isActive, int language = 1)
        {
            List<ProductCategoryTreeModel> result;
            if (IsCachingActivated)
            {
                var cacheKey = String.Format("BuildNavigation-{0}-{1}", isActive, language);
                result = DataCachingProvider.GetOrAdd(
                    cacheKey,
                    () => ProductCategoryRepository.BuildNavigation(isActive, language),
                    AppConfig.CacheMediumSeconds);
            }
            else
            {
                result = ProductCategoryRepository.BuildNavigation(isActive, language);
            }

            return result;
        }

        public List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1)
        {
            List<ProductCategoryTreeModel> result;
            if (IsCachingActivated)
            {
                var cacheKey = String.Format("ProductCategoryTree-{0}-{1}", isActive, language);
                result = DataCachingProvider.GetOrAdd(
                    cacheKey,
                    () => ProductCategoryRepository.BuildTree(isActive, language),
                    AppConfig.CacheMediumSeconds);
            }
            else
            {
                result = ProductCategoryRepository.BuildTree(isActive, language);
            }

            return result;
        }

        public async Task<List<ProductCategoryTreeModel>> BuildTreeAsync(bool? isActive, int language = 1)
        {
            if (IsCachingActivated)
            {
                var cacheKey = String.Format("ProductCategoryTree-{0}-{1}", isActive, language) + AsyncCacheKeySuffix;
                return await DataCachingProvider.GetOrAddAsync(
                    cacheKey,
                    () => ProductCategoryRepository.BuildTreeAsync(isActive, language),
                    AppConfig.CacheMediumSeconds).ConfigureAwait(false);
            }

            return await ProductCategoryRepository.BuildTreeAsync(isActive, language).ConfigureAwait(false);
        }

        public ProductCategory GetProductCategory(int categoryId)
        {
            ProductCategory result = ProductCategoryRepository.GetProductCategory(categoryId);
            return EntityFilterHelper.FilterProductCategory(result);
        }

        public async Task<ProductCategory> GetProductCategoryAsync(int categoryId)
        {
            ProductCategory result = await ProductCategoryRepository.GetProductCategoryAsync(categoryId).ConfigureAwait(false);
            return EntityFilterHelper.FilterProductCategory(result);
        }

        public List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language)
        {
            return ProductCategoryRepository.GetProductCategoryLeaves(isActive, language);
        }

        public async Task<List<ProductCategory>> GetProductCategoryLeavesAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductCategoryRepository.GetProductCategoryLeavesAsync(isActive, language, cancellationToken).ConfigureAwait(false);
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

        public async Task DeleteProductCategoriesAsync(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    await DeleteProductCategoryAsync(id).ConfigureAwait(false);
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

        public async Task DeleteProductCategoryAsync(int productCategoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var productCategory = await ProductCategoryRepository.GetProductCategoryAsync(productCategoryId, false).ConfigureAwait(false);
            var leaves = await GetProductCategoryLeavesAsync(null, productCategory.Lang, cancellationToken).ConfigureAwait(false);
            if (leaves.Any(r => r.Id == productCategoryId))
            {
                if (productCategory.MainImageId.HasValue)
                {
                    await FileStorageService.DeleteFileStorageAsync(productCategory.MainImageId.Value).ConfigureAwait(false);
                }

                var productIdList = productCategory.Products.Select(r => r.Id).ToList();
                foreach (var id in productIdList)
                {
                    await ProductService.DeleteProductByIdAsync(id, cancellationToken).ConfigureAwait(false);
                }

                await DeleteEntityAsync(productCategory).ConfigureAwait(false);
            }
        }

        public List<ProductCategory> GetMainPageProductCategories(int language)
        {
            var cacheKey = $"GetMainPageProductCategories-{language}";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => ProductCategoryRepository.GetMainPageProductCategories(language),
                AppConfig.CacheLongSeconds);
        }

        public async Task<List<ProductCategory>> GetMainPageProductCategoriesAsync(int language)
        {
            var cacheKey = $"GetMainPageProductCategories-{language}" + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => ProductCategoryRepository.GetMainPageProductCategoriesAsync(language),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public List<ProductCategory> GetAdminProductCategories(string search, int currentLanguage)
        {
            return ProductCategoryRepository.GetAdminProductCategories(search, currentLanguage);
        }

        public async Task<List<ProductCategory>> GetAdminProductCategoriesAsync(string search, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductCategoryRepository.GetAdminProductCategoriesAsync(search, currentLanguage, cancellationToken).ConfigureAwait(false);
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

        public async Task<List<ProductCategoryTreeModel>> GetBreadCrumbAsync(int productCategoryId, int language)
        {
            List<ProductCategoryTreeModel> result = new List<ProductCategoryTreeModel>();

            var tree = await BuildTreeAsync(true, language).ConfigureAwait(false);
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

        public ProductCategoryViewModel GetProductCategoryViewModel(int categoryId)
        {
            var result = new ProductCategoryViewModel();
            result.ProductCategory = GetProductCategory(categoryId);
            if (result.ProductCategory == null)
            {
                return null;
            }
            if (result.ProductCategory.ParentId > 0)
            {
                result.ProductCategory.Parent = GetProductCategory(result.ProductCategory.ParentId);
                if (result.ProductCategory.Parent != null && result.ProductCategory.Parent.ParentId > 0)
                {
                    result.ProductCategory.Parent.Parent = GetProductCategory(result.ProductCategory.Parent.ParentId);
                }
            }
            int lang = result.ProductCategory.Lang;
            List<Menu> lists = MenuService.GetActiveBaseContentsFromCache(true, lang);
            result.MainPageMenu = lists.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.ProductMenu = lists.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.Brands = BrandService.GetBrandsIfAnyProductExists(lang);
            result.ProductCategoryTree = BuildTree(true, lang);
            result.PriceFilterSetting = SettingService.GetSettingObjectByKey(Constants.ProductPriceFilterSetting);
            result.IsProductPriceEnable = SettingService.GetSettingObjectByKey(Constants.IsProductPriceEnable);
            result.IsProductReviewEnable = SettingService.GetSettingObjectByKey(Constants.IsProductReviewEnable);
            result.ChildrenProductCategories = ProductCategoryRepository.GetProductCategoriesByParentId(categoryId);
            result.CategoryChildrenProducts = ProductService.GetChildrenProducts(result.ProductCategory, result.ChildrenProductCategories);
            return result;
        }

        public async Task<ProductCategoryViewModel> GetProductCategoryViewModelAsync(int categoryId)
        {
            var result = new ProductCategoryViewModel();
            result.ProductCategory = await GetProductCategoryAsync(categoryId).ConfigureAwait(false);
            if (result.ProductCategory == null)
            {
                return null;
            }
            if (result.ProductCategory.ParentId > 0)
            {
                result.ProductCategory.Parent = await GetProductCategoryAsync(result.ProductCategory.ParentId).ConfigureAwait(false);
                if (result.ProductCategory.Parent != null && result.ProductCategory.Parent.ParentId > 0)
                {
                    result.ProductCategory.Parent.Parent = await GetProductCategoryAsync(result.ProductCategory.Parent.ParentId).ConfigureAwait(false);
                }
            }
            int lang = result.ProductCategory.Lang;
            List<Menu> lists = await MenuService.GetActiveBaseContentsFromCacheAsync(true, lang).ConfigureAwait(false);
            result.MainPageMenu = lists.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.ProductMenu = lists.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.Brands = await BrandService.GetBrandsIfAnyProductExistsAsync(lang).ConfigureAwait(false);
            result.ProductCategoryTree = await BuildTreeAsync(true, lang).ConfigureAwait(false);
            result.PriceFilterSetting = await SettingService.GetSettingObjectByKeyAsync(Constants.ProductPriceFilterSetting).ConfigureAwait(false);
            result.IsProductPriceEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductPriceEnable).ConfigureAwait(false);
            result.IsProductReviewEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductReviewEnable).ConfigureAwait(false);
            result.ChildrenProductCategories = await ProductCategoryRepository.GetProductCategoriesByParentIdAsync(categoryId).ConfigureAwait(false);
            result.CategoryChildrenProducts = await ProductService.GetChildrenProductsAsync(result.ProductCategory, result.ChildrenProductCategories).ConfigureAwait(false);
            return result;
        }
        public ProductCategoryDto GetProductCategoryDto(int productCategoryId)
        {
            var ProductCategory = GetProductCategory(productCategoryId);
            var result = Mapper.Map<ProductCategoryDto>(ProductCategory);
            return result;
        }

        public async Task<ProductCategoryDto> GetProductCategoryDtoAsync(int productCategoryId)
        {
            var productCategory = await GetProductCategoryAsync(productCategoryId).ConfigureAwait(false);
            var result = Mapper.Map<ProductCategoryDto>(productCategory);
            return result;
        }
    }
}