using EImece.Domain.Abstractions;
using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
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
        private readonly IBrandService BrandService;
        private readonly IProductRepository ProductRepository;
        private readonly IProductCategoryRepository ProductCategoryRepository;
        private readonly IMenuService MenuService;

        public ProductCategoryService(IProductCategoryRepository repository,
            IEimeceCacheProvider dataCachingProvider,
            ISettingService settingService,
            IFileStorageService fileStorageService,
            ICurrentUserContext currentUserContext,
            FilesHelper filesHelper,
            IBrandService brandService,
            IProductRepository productRepository,
            IMenuService menuService, ILogger<ProductCategoryService> logger)
            : base(repository, dataCachingProvider, settingService, fileStorageService, currentUserContext, filesHelper, logger)
        {
            ProductCategoryRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            BrandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
            ProductRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            MenuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        }

        /// <summary>
        /// Category active-entity/content lists live under the category: family so
        /// InvalidateCategoryCaches evicts them.
        /// </summary>
        protected override string ActiveListCachePrefix
        {
            get { return CacheKeys.CategoryPrefix; }
        }

        protected override void InvalidateCachesAfterMutation()
        {
            // Grid state/position/main-page toggles change the navigation tree and listings.
            InvalidateCategoryCaches();
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        [Timed("service.product_category.get_storefront_by_id")]

        public virtual async Task<StorefrontCategoryDto> GetStorefrontCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Category pages request the category plus its parent chain on every view; cache each
            // node briefly (medium TTL bounds product-count staleness). Invalidated by prefix in
            // InvalidateCategoryCaches().
            return await DataCachingProvider.GetOrAddAsync(
                CacheKeys.CategoryDetailAsync(categoryId),
                () => ProductCategoryRepository.GetStorefrontCategoryByIdAsync(categoryId, CancellationToken.None),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        [Timed("service.product_category.get_storefront_by_id_sync")]

        public virtual StorefrontCategoryDto GetStorefrontCategoryById(int categoryId)
        {
            return DataCachingProvider.GetOrAdd(
                CacheKeys.CategoryDetail(categoryId),
                () => ProductCategoryRepository.GetStorefrontCategoryById(categoryId),
                AppConfig.CacheMediumSeconds);
        }

        [Timed("service.product_category.get_main_page", "Time taken to get storefront main page categories")]
        public virtual async Task<List<StorefrontCategoryDto>> GetStorefrontMainPageCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.CategoryMainPageAsync(language);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => ProductCategoryRepository.GetStorefrontMainPageCategoriesAsync(language, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        [Timed("service.product_category.get_main_page_sync")]
        public virtual List<StorefrontCategoryDto> GetStorefrontMainPageCategories(int language)
        {
            var cacheKey = CacheKeys.CategoryMainPage(language);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => ProductCategoryRepository.GetStorefrontMainPageCategories(language),
                AppConfig.CacheLongSeconds);
        }

        [Timed("service.product_category.get_children", "Time taken to get storefront children categories")]
        public virtual async Task<List<StorefrontCategoryDto>> GetStorefrontChildrenCategoriesAsync(int parentCategoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await DataCachingProvider.GetOrAddAsync(
                CacheKeys.CategoryPrefix + "children:id" + parentCategoryId + AsyncCacheKeySuffix,
                () => ProductCategoryRepository.GetStorefrontChildrenCategoriesAsync(parentCategoryId, CancellationToken.None),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        [Timed("service.product_category.get_children_sync")]
        public virtual List<StorefrontCategoryDto> GetStorefrontChildrenCategories(int parentCategoryId)
        {
            return DataCachingProvider.GetOrAdd(
                CacheKeys.CategoryPrefix + "children:id" + parentCategoryId,
                () => ProductCategoryRepository.GetStorefrontChildrenCategories(parentCategoryId),
                AppConfig.CacheMediumSeconds);
        }

        [Timed("service.product_category.build_nav_tree")]

        public virtual async Task<List<StorefrontCategoryDto>> BuildStorefrontNavigationTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.CategoryPrefix + "navtree:lang" + language + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => ProductCategoryRepository.BuildStorefrontNavigationTreeAsync(language, cancellationToken),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        [Timed("service.product_category.build_nav_tree_sync")]

        public virtual List<StorefrontCategoryDto> BuildStorefrontNavigationTree(int language)
        {
            var cacheKey = CacheKeys.CategoryPrefix + "navtree:lang" + language;
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => ProductCategoryRepository.BuildStorefrontNavigationTree(language),
                AppConfig.CacheMediumSeconds);
        }

        [Timed("service.product_category.get_page_view_model")]

        public virtual async Task<ProductCategoryViewModel> GetStorefrontCategoryPageViewModelAsync(
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

            var priceFilterSetting = await SettingService.GetSettingValueDtoByKeyAsync(Constants.ProductPriceFilterSetting).ConfigureAwait(false);

            var result = new ProductCategoryViewModel();
            result.CategoryDto = categoryDto;
            result.ChildrenProductCategories = childCategories;

            List<int> brandIds = null;
            List<int> ratings = null;
            List<PriceRange> priceRanges = null;
            if (!string.IsNullOrEmpty(filter))
            {
                CategoryFilterHelper.ParseCategoryFilter(
                    filter,
                    priceFilterSetting,
                    out var bIds,
                    out var rList,
                    out var pRanges);

                if (bIds != null && bIds.Any()) brandIds = bIds;
                if (rList != null && rList.Any()) ratings = rList;
                if (pRanges != null && pRanges.Any()) priceRanges = pRanges;
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
                priceRanges,
                cancellationToken).ConfigureAwait(false);

            result.PagedProductDtos = pagedList;
            result.Page = page > 0 ? page : 1;
            result.RecordPerPage = recordPerPage;
            result.Filter = filter;
            result.Sorting = sorting;
            result.MinPrice = minPrice;
            result.MaxPrice = maxPrice;

            result.AllProducts = new List<StorefrontProductCardDto>();
            result.CategoryChildrenProducts = new List<StorefrontProductCardDto>();

            var mainPageDto = await MenuService.GetStorefrontPageByMenuLinkAsync(Constants.HomeIndexMenuLink, language, cancellationToken).ConfigureAwait(false);
            if (mainPageDto != null)
            {
                result.MainPageMenu = new StorefrontMenuDto { Id = mainPageDto.Id, Name = mainPageDto.Name, MenuLink = mainPageDto.MenuLink };
            }
            var productMenuDto = await MenuService.GetStorefrontPageByMenuLinkAsync(Constants.ProductsIndexMenuLink, language, cancellationToken).ConfigureAwait(false);
            if (productMenuDto != null)
            {
                result.ProductMenu = new StorefrontMenuDto { Id = productMenuDto.Id, Name = productMenuDto.Name, MenuLink = productMenuDto.MenuLink };
            }

            result.StorefrontBrands = await BrandService.GetStorefrontBrandsAsync(language, categoryId, cancellationToken).ConfigureAwait(false);
            result.ProductCategoryTree = await BuildTreeAsync(true, language).ConfigureAwait(false);
            result.PriceFilterSetting = priceFilterSetting;
            result.IsProductPriceEnable = await SettingService.GetSettingValueDtoByKeyAsync(Constants.IsProductPriceEnable).ConfigureAwait(false);
            result.IsProductReviewEnable = await SettingService.GetSettingValueDtoByKeyAsync(Constants.IsProductReviewEnable).ConfigureAwait(false);

            return result;
        }

        #endregion

        public List<ProductCategoryTreeModel> BuildNavigation(bool isActive, int language = 1)
        {
            List<ProductCategoryTreeModel> result;
            if (IsCachingActivated)
            {
                var cacheKey = CacheKeys.CategoryPrefix + "adminnav:" + isActive + ":lang" + language;
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

        [Timed("service.product_category.build_tree")]

        public virtual List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1)
        {
            // Admin passes isActive=null for live trees. Storefront/warmup pass true and may cache.
            if (!IsCachingActivated || !isActive.HasValue || DataCachingProvider == null)
            {
                return ProductCategoryRepository.BuildTree(isActive, language);
            }

            var cacheKey = CacheKeys.CategoryPrefix + "tree:" + isActive.Value + ":lang" + language;
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => ProductCategoryRepository.BuildTree(isActive, language),
                AppConfig.CacheMediumSeconds);
        }

        [Timed("service.product_category.build_tree_async")]

        public virtual async Task<List<ProductCategoryTreeModel>> BuildTreeAsync(bool? isActive, int language = 1)
        {
            // Admin passes isActive=null for live trees. Storefront/warmup pass true and may cache.
            if (!IsCachingActivated || !isActive.HasValue || DataCachingProvider == null)
            {
                return await ProductCategoryRepository.BuildTreeAsync(isActive, language).ConfigureAwait(false);
            }

            var cacheKey = CacheKeys.CategoryPrefix + "tree:" + isActive.Value + ":lang" + language + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => ProductCategoryRepository.BuildTreeAsync(isActive, language),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        [Timed("service.product_categories.get_by_id_sync")]
        public virtual ProductCategory GetProductCategory(int categoryId)
        {
            ProductCategory result = ProductCategoryRepository.GetProductCategory(categoryId);
            return EntityFilterHelper.FilterProductCategory(result);
        }

        [Timed("service.product_categories.get_by_id")]
        public virtual async Task<ProductCategory> GetProductCategoryAsync(int categoryId)
        {
            ProductCategory result = await ProductCategoryRepository.GetProductCategoryAsync(categoryId).ConfigureAwait(false);
            return EntityFilterHelper.FilterProductCategory(result);
        }

        [Timed("service.product_categories.get_leaves_sync")]
        public virtual List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language)
        {
            return ProductCategoryRepository.GetProductCategoryLeaves(isActive, language);
        }

        [Timed("service.product_categories.get_leaves")]
        public virtual async Task<List<ProductCategory>> GetProductCategoryLeavesAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken))
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
                Logger.LogError(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "DeleteBaseEntity :" + String.Join(",", values));
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
                Logger.LogError(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public void InvalidateCategoryCaches()
        {
            // Legacy TypeFullName-based keys written by deployments before the canonical
            // CacheKeys migration — kept so an in-flight cache is still dropped after upgrade.
            DataCachingProvider.ClearByPrefix("StorefrontMainPageCategories-");
            DataCachingProvider.ClearByPrefix("StorefrontNavigationTree-");
            DataCachingProvider.ClearByPrefix("ProductCategoryTree-");
            DataCachingProvider.ClearByPrefix("GetMainPageProductCategories-");
            DataCachingProvider.ClearByPrefix("Navigation-");
            // Canonical category: family (detail/children/tree/mainpage/active lists).
            DataCachingProvider.ClearByPrefix(CacheKeys.CategoryPrefix);
            DataCachingProvider.ClearByPrefix(CacheKeys.ProductListPrefix);
        }

        public override ProductCategory SaveOrEditEntity(ProductCategory entity)
        {
            var saved = base.SaveOrEditEntity(entity);
            InvalidateCategoryCaches();
            return saved;
        }

        public override async Task<ProductCategory> SaveOrEditEntityAsync(ProductCategory entity)
        {
            var saved = await base.SaveOrEditEntityAsync(entity).ConfigureAwait(false);
            InvalidateCategoryCaches();
            return saved;
        }

        public void DeleteProductCategory(int productCategoryId)
        {
            var productCategory = ProductCategoryRepository.GetProductCategory(productCategoryId, false);
            if (productCategory == null)
            {
                return;
            }

            if (productCategory.MainImageId.HasValue)
            {
                FileStorageService.DeleteFileStorage(productCategory.MainImageId.Value);
            }

            var productIdList = productCategory.Products?.Select(r => r.Id).ToList() ?? new List<int>();
            foreach (var id in productIdList)
            {
                ProductRepository.DeleteByWhereCondition(r => r.Id == id);
            }

            DeleteEntity(productCategory);
            InvalidateCategoryCaches();
        }

        public async Task DeleteProductCategoryAsync(int productCategoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var productCategory = await ProductCategoryRepository.GetProductCategoryAsync(productCategoryId, false).ConfigureAwait(false);
            if (productCategory == null)
            {
                return;
            }

            if (productCategory.MainImageId.HasValue)
            {
                await FileStorageService.DeleteFileStorageAsync(productCategory.MainImageId.Value).ConfigureAwait(false);
            }

            var productIdList = productCategory.Products?.Select(r => r.Id).ToList() ?? new List<int>();
            foreach (var id in productIdList)
            {
                await ProductRepository.DeleteByWhereConditionAsync(r => r.Id == id).ConfigureAwait(false);
            }

            await DeleteEntityAsync(productCategory).ConfigureAwait(false);
            InvalidateCategoryCaches();
        }

        public List<ProductCategory> GetMainPageProductCategories(int language)
        {
            var cacheKey = CacheKeys.CategoryPrefix + "mainpageentities:lang" + language;
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => ProductCategoryRepository.GetMainPageProductCategories(language),
                AppConfig.CacheLongSeconds);
        }

        public async Task<List<ProductCategory>> GetMainPageProductCategoriesAsync(int language)
        {
            var cacheKey = CacheKeys.CategoryPrefix + "mainpageentities:lang" + language + AsyncCacheKeySuffix;
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

        [Timed("service.product_category.get_breadcrumb")]

        public virtual async Task<List<ProductCategoryTreeModel>> GetBreadCrumbAsync(int productCategoryId, int language)
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

        [Timed("service.product_categories.get_category_view_model")]
        public virtual async Task<ProductCategoryViewModel> GetProductCategoryViewModelAsync(int categoryId)
        {
            var categoryDto = await ProductCategoryRepository.GetStorefrontCategoryByIdAsync(categoryId).ConfigureAwait(false);
            if (categoryDto == null) return null;
            return await GetStorefrontCategoryPageViewModelAsync(categoryId, 1, Models.Enums.SortingType.Default, null, null, null, Constants.DefaultCategoryPageSize, categoryDto.Lang).ConfigureAwait(false);
        }

        [Timed("service.product_categories.get_dto_sync")]
        public virtual ProductCategoryDto GetProductCategoryDto(int productCategoryId)
        {
            return ProductCategoryRepository.GetProductCategoryDto(productCategoryId);
        }

        [Timed("service.product_categories.get_dto")]
        public virtual async Task<ProductCategoryDto> GetProductCategoryDtoAsync(int productCategoryId)
        {
            return await ProductCategoryRepository.GetProductCategoryDtoAsync(productCategoryId).ConfigureAwait(false);
        }
    }
}