using EImece.Domain.Caching;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace EImece.Domain.Services
{
    public class ProductService : BaseContentService<Product>, IProductService
    {
        private static readonly Logger ProductServiceLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }

        [Inject]
        public IProductCommentRepository ProductCommentRepository { get; set; }

        [Inject]
        public IOrderProductRepository OrderProductRepository { get; set; }

        [Inject]
        public ITagService TagService { get; set; }

        [Inject]
        public IStoryService StoryService { get; set; }

        [Inject]
        public ITemplateService TemplateService { get; set; }

        [Inject]
        public IStoryRepository StoryRepository { get; set; }

        private IProductRepository ProductRepository { get; set; }

        public ProductService(IProductRepository repository) : base(repository)
        {
            ProductRepository = repository;
        }

        /// <summary>
        /// Product active-list and active-entity caches live under the product:list family so
        /// InvalidateProductListCaches (run after every mutating admin action) evicts them.
        /// </summary>
        protected override string ActiveListCachePrefix
        {
            get { return CacheKeys.ProductListPrefix; }
        }

        protected override void InvalidateCachesAfterMutation()
        {
            // Grid state/position/main-page/campaign toggles change storefront listings.
            InvalidateProductListCaches();
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        /// <summary>
        /// Shared, customer-independent product-detail DTO (product + files + tags + specs +
        /// approved comments). Cached under <c>product:detail:</c> so authenticated users and
        /// OutputCache misses stop rebuilding the ~10-query projection on every view. Dropped by
        /// InvalidateProductListCaches on any product mutation and by comment moderation.
        /// </summary>
        [Timed("service.products.get_storefront_detail", "Time taken to get storefront product detail")]
        public virtual async Task<StorefrontProductDetailDto> GetStorefrontProductDetailAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (DataCachingProvider == null)
            {
                return await ProductRepository.GetStorefrontProductDetailByIdAsync(id, cancellationToken).ConfigureAwait(false);
            }
            return await DataCachingProvider.GetOrAddAsync(
                CacheKeys.ProductDetailAsync(id),
                () => ProductRepository.GetStorefrontProductDetailByIdAsync(id, CancellationToken.None),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        [Timed("service.products.get_storefront_detail_sync", "Time taken to get storefront product detail (sync)")]
        public virtual StorefrontProductDetailDto GetStorefrontProductDetail(int id)
        {
            // Null provider (manual construction / unit tests) bypasses the cache, matching
            // the SettingService convention.
            if (DataCachingProvider == null)
            {
                return ProductRepository.GetStorefrontProductDetailById(id);
            }
            return DataCachingProvider.GetOrAdd(
                CacheKeys.ProductDetail(id),
                () => ProductRepository.GetStorefrontProductDetailById(id),
                AppConfig.CacheMediumSeconds);
        }

        [Timed("service.products.get_active_async", "Time taken to get active products")]
        public virtual async Task<List<StorefrontProductCardDto>> GetStorefrontActiveProductsAsync(int? language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetStorefrontActiveProductsAsync(language, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.products.get_active_sync")]
        public virtual List<StorefrontProductCardDto> GetStorefrontActiveProducts(int? language)
        {
            return ProductRepository.GetStorefrontActiveProducts(language);
        }

        public async Task<PaginatedList<StorefrontProductCardDto>> GetStorefrontActiveProductsPagedAsync(int pageIndex, int pageSize, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetStorefrontActiveProductsPagedAsync(pageIndex, pageSize, lang, cancellationToken).ConfigureAwait(false);
        }

        public PaginatedList<StorefrontProductCardDto> GetStorefrontActiveProductsPaged(int pageIndex, int pageSize, int lang)
        {
            return ProductRepository.GetStorefrontActiveProductsPaged(pageIndex, pageSize, lang);
        }

        public async Task<PaginatedList<StorefrontProductCardDto>> GetStorefrontMainPageProductsPagedAsync(int pageIndex, int pageSize, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetStorefrontActiveProductsPagedAsync(pageIndex, pageSize, lang, cancellationToken).ConfigureAwait(false);
        }

        public PaginatedList<StorefrontProductCardDto> GetStorefrontMainPageProductsPaged(int pageIndex, int pageSize, int lang)
        {
            return ProductRepository.GetStorefrontActiveProductsPaged(pageIndex, pageSize, lang);
        }

        [Timed("service.products.search_storefront_async", "Time taken to search storefront products")]
        public virtual async Task<PaginatedList<StorefrontProductCardDto>> SearchStorefrontProductsAsync(int pageIndex, int pageSize, string search, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.SearchStorefrontProductsAsync(pageIndex, pageSize, search, lang, sorting, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.products.search_storefront_sync")]
        public virtual PaginatedList<StorefrontProductCardDto> SearchStorefrontProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting)
        {
            return ProductRepository.SearchStorefrontProducts(pageIndex, pageSize, search, lang, sorting);
        }

        [Timed("service.products.get_by_tag", "Time taken to get storefront products by tag id")]
        public virtual async Task<PaginatedList<StorefrontProductCardDto>> GetStorefrontProductsByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetStorefrontProductsByTagIdAsync(tagId, pageIndex, pageSize, lang, sorting, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.products.get_by_tag_sync")]
        public virtual PaginatedList<StorefrontProductCardDto> GetStorefrontProductsByTagId(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting)
        {
            return ProductRepository.GetStorefrontProductsByTagId(tagId, pageIndex, pageSize, lang, sorting);
        }

        [Timed("service.products.get_related", "Time taken to get storefront related products")]
        public virtual async Task<List<StorefrontProductCardDto>> GetStorefrontRelatedProductsAsync(int[] tagIds, int take, int language, int excludedProductId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetStorefrontRelatedProductsAsync(tagIds, take, language, excludedProductId, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.products.get_related_sync")]
        public virtual List<StorefrontProductCardDto> GetStorefrontRelatedProducts(int[] tagIds, int take, int language, int excludedProductId)
        {
            return ProductRepository.GetStorefrontRelatedProducts(tagIds, take, language, excludedProductId);
        }

        [Timed("service.products.get_category_products", "Time taken to get storefront category products")]
        public virtual async Task<List<StorefrontProductCardDto>> GetStorefrontCategoryProductsAsync(int categoryId, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetStorefrontCategoryProductsAsync(categoryId, language, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.products.get_category_products_sync")]
        public virtual List<StorefrontProductCardDto> GetStorefrontCategoryProducts(int categoryId, int language)
        {
            return ProductRepository.GetStorefrontCategoryProducts(categoryId, language);
        }

        [Timed("service.products.get_main_page_products", "Time taken to get storefront main page products")]
        public virtual async Task<List<StorefrontProductCardDto>> GetStorefrontMainPageProductsAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetStorefrontMainPageProductsAsync(take, language, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.products.get_main_page_products_sync")]
        public virtual List<StorefrontProductCardDto> GetStorefrontMainPageProducts(int take, int language)
        {
            return ProductRepository.GetStorefrontMainPageProducts(take, language);
        }

        [Timed("service.products.get_latest_products", "Time taken to get storefront latest products")]
        public virtual async Task<List<StorefrontProductCardDto>> GetStorefrontLatestProductsAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetStorefrontLatestProductsAsync(take, language, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.products.get_latest_products_sync")]
        public virtual List<StorefrontProductCardDto> GetStorefrontLatestProducts(int take, int language)
        {
            return ProductRepository.GetStorefrontLatestProducts(take, language);
        }

        [Timed("service.products.get_campaign_products", "Time taken to get storefront campaign products")]
        public virtual async Task<List<StorefrontProductCardDto>> GetStorefrontCampaignProductsAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetStorefrontCampaignProductsAsync(take, language, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.products.get_campaign_products_sync")]
        public virtual List<StorefrontProductCardDto> GetStorefrontCampaignProducts(int take, int language)
        {
            return ProductRepository.GetStorefrontCampaignProducts(take, language);
        }

        #endregion

        public List<Product> GetAdminPageList(int id, string search, int lang)
        {
            return ProductRepository.GetAdminPageList(id, search, lang);
        }

        public List<Product> GetAdminPageList(int id, int brandId, string search, int lang)
        {
            return ProductRepository.GetAdminPageList(id, brandId, search, lang);
        }

        public List<Product> GetAdminPageList(int id, int brandId, string search, int lang, ProductAdminListFilter filter)
        {
            return ProductRepository.GetAdminPageList(id, brandId, search, lang, filter);
        }

        public async Task<List<Product>> GetAdminPageListAsync(int id, string search, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetAdminPageListAsync(id, search, lang, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Product>> GetAdminPageListAsync(int id, int brandId, string search, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetAdminPageListAsync(id, brandId, search, lang, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Product>> GetAdminPageListAsync(int id, int brandId, string search, int lang, ProductAdminListFilter filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetAdminPageListAsync(id, brandId, search, lang, filter, cancellationToken).ConfigureAwait(false);
        }

        public string UpdatePrices(UpdatePriceRequest request)
        {
            var result = ProductRepository.UpdateProductPrices(request);
            if (!"hata".Equals(result, StringComparison.Ordinal))
            {
                InvalidateProductListCaches();
            }
            return result;
        }

        public async Task<string> UpdatePricesAsync(UpdatePriceRequest request)
        {
            var result = await ProductRepository.UpdateProductPricesAsync(request).ConfigureAwait(false);
            if (!"hata".Equals(result, StringComparison.Ordinal))
            {
                InvalidateProductListCaches();
            }
            return result;
        }

        [Timed("service.products.get_main_page", "Time taken to build main page view model")]
        public virtual ProductIndexViewModel GetMainPageProducts(int pageIndex, int lang)
        {
            var cacheKey = CacheKeys.MainPageProducts(pageIndex, lang);

            // Absolute expiration: catalogue pages must refresh within CacheMediumSeconds even when
            // they stay hot. Single-flight: one DB build per (page, language) after expiry.
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () =>
                {
                    var result = new ProductIndexViewModel();
                    int pageSize = AppConfig.RecordPerPage;
                    result.CompanyName = SettingService.GetSettingValueDtoByKey(Constants.CompanyName);
                    var menus = MenuService.GetActiveBaseContentsFromCache(true, lang);
                    var mainPageMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
                    var productMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
                    result.MainPageMenu = StorefrontMenuDto.FromEntity(mainPageMenu);
                    result.ProductMenu = StorefrontMenuDto.FromEntity(productMenu);

                    result.Products = ProductRepository.GetStorefrontActiveProductsPaged(pageIndex, pageSize, lang);
                    result.Tags = TagService.GetActiveBaseEntities(true, lang).Select(t => StorefrontTagDto.FromEntity(t)).ToList();
                    return result;
                },
                CachePolicy.Absolute(AppConfig.CacheMediumSeconds));
        }

        /// <summary>
        /// Async twin of <see cref="GetMainPageProducts"/>. Every leg that touches SQL Server is
        /// awaited, so the request thread is released for the whole duration of the page build
        /// instead of being parked on four sequential blocking round trips.
        ///
        /// The language-scoped parts (company name, menus, tags) come from their own single-flight
        /// caches; only the paged product query runs per request, which is what lets the caller's
        /// CancellationToken reach the database. Whole-page caching is the controller's job
        /// (CustomOutputCache) rather than a shared view-model cache entry, because a shared entry
        /// cannot honour a per-request token.
        /// </summary>
        [Timed("service.products.get_main_page_async")]
        public virtual async Task<ProductIndexViewModel> GetMainPageProductsAsync(int pageIndex, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new ProductIndexViewModel();
            int pageSize = AppConfig.RecordPerPage;

            result.CompanyName = await SettingService.GetSettingValueDtoByKeyAsync(Constants.CompanyName).ConfigureAwait(false);

            var menus = await MenuService.GetStorefrontActiveMenusCachedAsync(lang).ConfigureAwait(false);
            result.MainPageMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.ProductMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));

            result.Products = await ProductRepository.GetStorefrontActiveProductsPagedAsync(pageIndex, pageSize, lang, cancellationToken).ConfigureAwait(false);
            var tags = await TagService.GetStorefrontProductTagsCachedAsync(lang).ConfigureAwait(false);
            result.Tags = tags;

            return result;
        }

        public void SaveProductTags(int id, int[] tags)
        {
            ProductTagRepository.SaveProductTags(id, tags);
            // Tag relations feed related-products and products-by-tag listings.
            InvalidateProductListCaches();
            TagService.InvalidateTagCaches();
        }

        public async Task SaveProductTagsAsync(int id, int[] tags)
        {
            await ProductTagRepository.SaveProductTagsAsync(id, tags).ConfigureAwait(false);
            InvalidateProductListCaches();
            TagService.InvalidateTagCaches();
        }

        public List<ProductTag> GetProductTagsByProductId(int productId)
        {
            return ProductTagRepository.GetAllByProductId(productId);
        }

        public async Task<List<ProductTag>> GetProductTagsByProductIdAsync(int productId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductTagRepository.GetAllByProductIdAsync(productId, cancellationToken).ConfigureAwait(false);
        }

        public ProductAdminModel GetProductAdminPage(int categoryId, String search, int lang, int productId)
        {
            var result = new ProductAdminModel();
            result.Products = this.GetAdminPageList(categoryId, search, lang);
            result.ProductCategoryTree = ProductCategoryService.BuildTree(null, lang);

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

        [Timed("service.products.get_detail_view_model_sync")]
        public virtual ProductDetailViewModel GetProductDetailViewModelById(int id)
        {
            var result = new ProductDetailViewModel();
            var productDto = GetStorefrontProductDetail(id);

            if (productDto == null)
            {
                return null;
            }
            if (!productDto.IsActive)
            {
                result.ProductDto = productDto;
                return result;
            }
            result.ProductDto = productDto;
            result.IsProductPriceEnable = SettingService.GetSettingValueDtoByKey(Constants.IsProductPriceEnable);
            result.IsProductReviewEnable = SettingService.GetSettingValueDtoByKey(Constants.IsProductReviewEnable);
            result.PaymentDetailHtml = SettingService.GetSettingValueDtoByKey(Constants.PaymentDetailHtml);
            result.WhatsAppCommunicationLink = SettingService.GetSettingValueDtoByKey(Constants.WhatsAppCommunicationLink);
            result.CompanyName = SettingService.GetSettingValueDtoByKey(Constants.CompanyName);
            result.Contact = ContactUsFormViewModel.CreateContactUsFormViewModel("productDetail", id, EImeceItemType.Product);
            result.CargoDescription = SettingService.GetSettingValueDtoByKey(Constants.CargoDescription, productDto.Lang);
            result.CargoPrice = SettingService.GetSettingValueDtoByKey(Constants.CargoPrice, productDto.Lang);
            var mainPageDto = MenuService.GetStorefrontPageByMenuLink(Constants.HomeIndexMenuLink, productDto.Lang);
            if (mainPageDto != null)
            {
                result.MainPageMenu = new StorefrontMenuDto { Id = mainPageDto.Id, Name = mainPageDto.Name, MenuLink = mainPageDto.MenuLink };
            }
            var productMenuDto = MenuService.GetStorefrontPageByMenuLink(Constants.ProductsIndexMenuLink, productDto.Lang);
            if (productMenuDto != null)
            {
                result.ProductMenu = new StorefrontMenuDto { Id = productMenuDto.Id, Name = productMenuDto.Name, MenuLink = productMenuDto.MenuLink };
            }
            result.SocialMediaLinks = SettingService.CreateShareableSocialMediaLinks(productDto.DetailPageAbsoluteUrl, productDto.NameLong, productDto.ImageFullPath(1000, 0));
            if (productDto.ProductCategoryTemplateId.HasValue)
            {
                var tmplXml = TemplateService.GetTemplateXml(productDto.ProductCategoryTemplateId.Value);
                if (!string.IsNullOrWhiteSpace(tmplXml))
                {
                    result.Template = new Models.DTOs.Storefront.TemplateXmlDto { TemplateXml = tmplXml };
                }
            }
            result.BreadCrumb = ProductCategoryService.GetBreadCrumb(productDto.ProductCategoryId, productDto.Lang);
            result.RelatedStories = new List<StorefrontStoryCardDto>();
            int relatedProductTake = 20;
            result.RelatedProducts = new List<StorefrontProductCardDto>();
            if (productDto.ProductTags != null && productDto.ProductTags.Any())
            {
                var tagIdList = productDto.ProductTags.Select(t => t.Id).ToArray();
                result.RelatedProducts = ProductRepository.GetStorefrontRelatedProducts(tagIdList, relatedProductTake, productDto.Lang, id);
            }

            if (result.RelatedProducts.Count < 20)
            {
                relatedProductTake -= result.RelatedProducts.Count;
                result.RelatedProducts.AddRange(
                    ProductRepository.GetStorefrontRandomProductsByCategoryId(productDto.ProductCategoryId, relatedProductTake, productDto.Lang, id));
            }

            result.RelatedProducts = result.RelatedProducts.GroupBy(p => p.Id).Select(g => g.First()).Take(20).ToList();

            return result;
        }

        public async Task<ProductDetailViewModel> GetProductDetailViewModelByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new ProductDetailViewModel();
            var productDto = await GetStorefrontProductDetailAsync(id, cancellationToken).ConfigureAwait(false);

            if (productDto == null)
            {
                return null;
            }
            if (!productDto.IsActive)
            {
                result.ProductDto = productDto;
                return result;
            }
            result.ProductDto = productDto;
            result.IsProductPriceEnable = await SettingService.GetSettingValueDtoByKeyAsync(Constants.IsProductPriceEnable).ConfigureAwait(false);
            result.IsProductReviewEnable = await SettingService.GetSettingValueDtoByKeyAsync(Constants.IsProductReviewEnable).ConfigureAwait(false);
            result.PaymentDetailHtml = await SettingService.GetSettingValueDtoByKeyAsync(Constants.PaymentDetailHtml).ConfigureAwait(false);
            result.WhatsAppCommunicationLink = await SettingService.GetSettingValueDtoByKeyAsync(Constants.WhatsAppCommunicationLink).ConfigureAwait(false);
            result.CompanyName = await SettingService.GetSettingValueDtoByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            result.Contact = ContactUsFormViewModel.CreateContactUsFormViewModel("productDetail", id, EImeceItemType.Product);
            result.CargoDescription = await SettingService.GetSettingValueDtoByKeyAsync(Constants.CargoDescription, productDto.Lang).ConfigureAwait(false);
            result.CargoPrice = await SettingService.GetSettingValueDtoByKeyAsync(Constants.CargoPrice, productDto.Lang).ConfigureAwait(false);
            var mainPageDtoAsync = await MenuService.GetStorefrontPageByMenuLinkAsync(Constants.HomeIndexMenuLink, productDto.Lang, cancellationToken).ConfigureAwait(false);
            if (mainPageDtoAsync != null)
            {
                result.MainPageMenu = new StorefrontMenuDto { Id = mainPageDtoAsync.Id, Name = mainPageDtoAsync.Name, MenuLink = mainPageDtoAsync.MenuLink };
            }
            var productMenuDtoAsync = await MenuService.GetStorefrontPageByMenuLinkAsync(Constants.ProductsIndexMenuLink, productDto.Lang, cancellationToken).ConfigureAwait(false);
            if (productMenuDtoAsync != null)
            {
                result.ProductMenu = new StorefrontMenuDto { Id = productMenuDtoAsync.Id, Name = productMenuDtoAsync.Name, MenuLink = productMenuDtoAsync.MenuLink };
            }
            result.SocialMediaLinks = SettingService.CreateShareableSocialMediaLinks(productDto.DetailPageAbsoluteUrl, productDto.NameLong, productDto.ImageFullPath(1000, 0));
            if (productDto.ProductCategoryTemplateId.HasValue)
            {
                var tmplXmlAsync = await TemplateService.GetTemplateXmlAsync(productDto.ProductCategoryTemplateId.Value, cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(tmplXmlAsync))
                {
                    result.Template = new Models.DTOs.Storefront.TemplateXmlDto { TemplateXml = tmplXmlAsync };
                }
            }
            result.BreadCrumb = await ProductCategoryService.GetBreadCrumbAsync(productDto.ProductCategoryId, productDto.Lang).ConfigureAwait(false);
            result.RelatedStories = new List<StorefrontStoryCardDto>();
            int relatedProductTake = 20;
            result.RelatedProducts = new List<StorefrontProductCardDto>();
            if (productDto.ProductTags != null && productDto.ProductTags.Any())
            {
                var tagIdList = productDto.ProductTags.Select(t => t.Id).ToArray();
                result.RelatedProducts = await GetStorefrontRelatedProductsAsync(tagIdList, relatedProductTake, productDto.Lang, id, cancellationToken).ConfigureAwait(false);
            }

            if (result.RelatedProducts.Count < 20)
            {
                relatedProductTake -= result.RelatedProducts.Count;
                result.RelatedProducts.AddRange(
                    await ProductRepository.GetStorefrontRandomProductsByCategoryIdAsync(productDto.ProductCategoryId, relatedProductTake, productDto.Lang, id, cancellationToken).ConfigureAwait(false));
            }

            result.RelatedProducts = result.RelatedProducts.GroupBy(p => p.Id).Select(g => g.First()).Take(20).ToList();

            return result;
        }

        private List<Product> GetRandomProductsByCategoryId(int productCategoryId, int relatedProductTake, int lang, int id)
        {
            List<Product> result = null;

            result = ProductRepository.GetRandomProductsByCategoryId(productCategoryId, relatedProductTake * 3, lang, id);

            return result;
        }

        private async Task<List<Product>> GetRandomProductsByCategoryIdAsync(int productCategoryId, int relatedProductTake, int lang, int id, CancellationToken cancellationToken)
        {
            return await ProductRepository.GetRandomProductsByCategoryIdAsync(productCategoryId, relatedProductTake * 3, lang, id, cancellationToken).ConfigureAwait(false);
        }

        private List<Product> GetRelatedProducts(int[] tagIdList, int relatedProductTake, int lang, int id)
        {
            List<Product> result = null;

            result = ProductRepository.GetRelatedProducts(tagIdList, relatedProductTake * 3, lang, id);

            return result;
        }

        private async Task<List<Product>> GetRelatedProductsAsync(int[] tagIdList, int relatedProductTake, int lang, int id, CancellationToken cancellationToken)
        {
            return await ProductRepository.GetRelatedProductsAsync(tagIdList, relatedProductTake * 3, lang, id, cancellationToken).ConfigureAwait(false);
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

        public virtual new async Task DeleteBaseEntityAsync(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    await DeleteProductByIdAsync(id).ConfigureAwait(false);
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

        public ProductDeleteResult DeleteProductById(int id)
        {
            try
            {
                var product = ProductRepository.GetProduct(id);
                if (product == null)
                {
                    return ProductDeleteResult.Failed;
                }

                // Preserve historical order data: unlink ProductId from existing OrderProducts
                var relatedOrderProducts = OrderProductRepository.FindBy(r => r.ProductId == id).ToList();
                bool hasOrderHistory = relatedOrderProducts.Count > 0;
                foreach (var op in relatedOrderProducts)
                {
                    op.ProductId = null;
                    OrderProductRepository.SaveOrEdit(op);
                }

                ProductCommentRepository?.DeleteByWhereCondition(r => r.ProductId == id);
                ProductSpecificationRepository?.DeleteByWhereCondition(r => r.ProductId == id);
                ProductTagRepository?.DeleteByWhereCondition(r => r.ProductId == id);

                // Preserve media image files if product was ordered so historical order views can render them
                if (!hasOrderHistory)
                {
                    FileStorageService?.DeleteGalleryImages(id, MediaModType.Products);
                    if (product.MainImageId.HasValue && FileStorageService != null)
                    {
                        FileStorageService.DeleteFileStorage(product.MainImageId.Value);
                    }
                }
                DeleteEntity(product);
                InvalidateProductListCaches();
                return ProductDeleteResult.Deleted;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("DeleteProductById exception: " + e);
                ProductServiceLogger.Error(e, "DeleteProductById did not work for productId:" + id);
                return ProductDeleteResult.Failed;
            }
        }

        public async Task<ProductDeleteResult> DeleteProductByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var product = await ProductRepository.GetProductAsync(id, cancellationToken).ConfigureAwait(false);
                if (product == null)
                {
                    return ProductDeleteResult.Failed;
                }

                // Preserve historical order data: unlink ProductId from existing OrderProducts
                var relatedQuery = OrderProductRepository.FindBy(r => r.ProductId == id);
                List<OrderProduct> relatedOrderProducts;
                if (relatedQuery is System.Data.Entity.Infrastructure.IDbAsyncEnumerable<OrderProduct>)
                {
                    relatedOrderProducts = await relatedQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    relatedOrderProducts = relatedQuery.ToList();
                }

                bool hasOrderHistory = relatedOrderProducts.Count > 0;
                foreach (var op in relatedOrderProducts)
                {
                    op.ProductId = null;
                    await OrderProductRepository.SaveOrEditAsync(op).ConfigureAwait(false);
                }

                if (ProductCommentRepository != null)
                    await ProductCommentRepository.DeleteByWhereConditionAsync(r => r.ProductId == id).ConfigureAwait(false);
                if (ProductSpecificationRepository != null)
                    await ProductSpecificationRepository.DeleteByWhereConditionAsync(r => r.ProductId == id).ConfigureAwait(false);
                if (ProductTagRepository != null)
                    await ProductTagRepository.DeleteByWhereConditionAsync(r => r.ProductId == id).ConfigureAwait(false);

                // Preserve media image files if product was ordered so historical order views can render them
                if (!hasOrderHistory)
                {
                    if (FileStorageService != null)
                        await FileStorageService.DeleteGalleryImagesAsync(id, MediaModType.Products).ConfigureAwait(false);
                    if (product.MainImageId.HasValue && FileStorageService != null)
                    {
                        await FileStorageService.DeleteFileStorageAsync(product.MainImageId.Value).ConfigureAwait(false);
                    }
                }
                await DeleteEntityAsync(product).ConfigureAwait(false);
                InvalidateProductListCaches();
                return ProductDeleteResult.Deleted;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("DeleteProductByIdAsync exception: " + e);
                ProductServiceLogger.Error(e, "DeleteProductById did not work for productId:" + id);
                return ProductDeleteResult.Failed;
            }
        }

        [Timed("service.products.search_sync")]
        public virtual ProductsSearchViewModel SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting)
        {
            // Absolute + short TTL: search cardinality is high; keep entries brief and invalidate
            // the whole search prefix when any product changes (see InvalidateProductListCaches).
            var cacheKey = CacheKeys.ProductSearch(search, pageIndex, pageSize, lang, sorting);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => BuildProductsSearchViewModel(pageIndex, pageSize, search, lang, sorting),
                CachePolicy.Absolute(AppConfig.CacheShortSeconds));
        }

        [Timed("service.products.search")]
        public virtual async Task<ProductsSearchViewModel> SearchProductsAsync(int pageIndex, int pageSize, string search, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.ProductSearchAsync(search, pageIndex, pageSize, lang, sorting);
            // CancellationToken.None inside the factory: the cached result is shared across requests,
            // so it must not be tied to one caller's lifetime (see AsyncCacheKeySuffix notes).
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => BuildProductsSearchViewModelAsync(pageIndex, pageSize, search, lang, sorting, CancellationToken.None),
                CachePolicy.Absolute(AppConfig.CacheShortSeconds)).ConfigureAwait(false);
        }

        private ProductsSearchViewModel BuildProductsSearchViewModel(int pageIndex, int pageSize, string search, int lang, SortingType sorting)
        {
            var r = new ProductsSearchViewModel();
            r.Search = search;
            if (!String.IsNullOrEmpty(search))
            {
                r.Products = ProductRepository.SearchStorefrontProducts(pageIndex, pageSize, search, lang, sorting);
            }
            else
            {
                r.Products = new PaginatedList<StorefrontProductCardDto>(new List<StorefrontProductCardDto>(), pageIndex, pageSize, 0);
            }

            var menus = MenuService.GetActiveBaseContentsFromCache(true, lang);
            var mainPageMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            var productMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            r.MainPageMenu = StorefrontMenuDto.FromEntity(mainPageMenu);
            r.ProductMenu = StorefrontMenuDto.FromEntity(productMenu);

            return r;
        }

        private async Task<ProductsSearchViewModel> BuildProductsSearchViewModelAsync(int pageIndex, int pageSize, string search, int lang, SortingType sorting, CancellationToken cancellationToken)
        {
            var r = new ProductsSearchViewModel();
            r.Search = search;
            if (!String.IsNullOrEmpty(search))
            {
                r.Products = await ProductRepository.SearchStorefrontProductsAsync(pageIndex, pageSize, search, lang, sorting, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                r.Products = new PaginatedList<StorefrontProductCardDto>(new List<StorefrontProductCardDto>(), pageIndex, pageSize, 0);
            }

            var menus = await MenuService.GetStorefrontActiveMenusCachedAsync(lang).ConfigureAwait(false);
            r.MainPageMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            r.ProductMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));

            return r;
        }

        public void SaveProductSpecifications(List<ProductSpecification> specifications, int productId)
        {
            if (specifications.IsNotEmpty())
            {
                ProductSpecificationRepository.DeleteByWhereCondition(r => r.ProductId == productId);
                foreach (var item in specifications)
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        ProductSpecificationRepository.Add(item);
                    }
                }

                ProductSpecificationRepository.Save();
            }
        }

        public async Task SaveProductSpecificationsAsync(List<ProductSpecification> specifications, int productId)
        {
            if (specifications.IsNotEmpty())
            {
                await ProductSpecificationRepository.DeleteByWhereConditionAsync(r => r.ProductId == productId).ConfigureAwait(false);
                foreach (var item in specifications.Where(s => !string.IsNullOrEmpty(s.Value)))
                {
                    ProductSpecificationRepository.Add(item);
                }

                await ProductSpecificationRepository.SaveAsync().ConfigureAwait(false);
            }
        }

        public List<Product> GetActiveProducts(int? language)
        {
            var cacheKey = CacheKeys.ActiveProducts(language);
            // Absolute expiration keeps the active-product set fresh within CacheMediumSeconds.
            // Single-flight prevents a DB thundering-herd when this hot key expires.
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => ProductRepository.GetActiveProducts(language),
                CachePolicy.Absolute(AppConfig.CacheMediumSeconds));
        }

        public async Task<List<Product>> GetActiveProductsAsync(int? language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.ActiveProductsAsync(language);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => ProductRepository.GetActiveProductsAsync(language, CancellationToken.None),
                CachePolicy.Absolute(AppConfig.CacheMediumSeconds)).ConfigureAwait(false);
        }

        /// <summary>
        /// Drops every storefront cache entry that a product mutation can affect: list/search
        /// pages, cached detail DTOs (embed tags/specs/comments/files), category detail/children
        /// DTOs with their active-product counts, and tag listings (product-tag relations).
        /// Called after product save/delete/state-change/move/tag-edit so the next storefront
        /// request rebuilds from SQL instead of serving stale AbsoluteExpiration data.
        /// </summary>
        public void InvalidateProductListCaches()
        {
            if (DataCachingProvider == null)
            {
                return;
            }
            var listRemoved = DataCachingProvider.ClearByPrefix(CacheKeys.ProductListPrefix);
            var searchRemoved = DataCachingProvider.ClearByPrefix(CacheKeys.ProductSearchPrefix);
            var detailRemoved = DataCachingProvider.ClearByPrefix(CacheKeys.ProductDetailPrefix);
            var relatedRemoved = DataCachingProvider.ClearByPrefix(CacheKeys.ProductRelatedPrefix);
            var categoryRemoved = DataCachingProvider.ClearByPrefix(CacheKeys.CategoryPrefix);
            ProductServiceLogger.Info(
                "InvalidateProductListCaches removed {0} list + {1} search + {2} detail + {3} related + {4} category entries",
                listRemoved,
                searchRemoved,
                detailRemoved,
                relatedRemoved,
                categoryRemoved);
        }

        public override Product SaveOrEditEntity(Product entity)
        {
            var saved = base.SaveOrEditEntity(entity);
            InvalidateProductListCaches();
            return saved;
        }

        public override async Task<Product> SaveOrEditEntityAsync(Product entity)
        {
            var saved = await base.SaveOrEditEntityAsync(entity).ConfigureAwait(false);
            InvalidateProductListCaches();
            return saved;
        }

        public Rss20FeedFormatter GetProductsRss(RssParams rssParams)
        {
            var items = ProductRepository.GetActiveProductsForRss(rssParams.Language, rssParams.Take);
            var request = HttpContextFactory.Create().Request;
            var builder = new UriBuilder(AppConfig.HttpProtocol, request.Url.Host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));

            String title = SettingService.GetSettingByKey(Constants.CompanyName);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

            var urlHelper = new UrlHelper(request.RequestContext);
            String atomSelfHref = urlHelper.Action("products", "rss", new { rssParams.Take, rssParams.Language }, AppConfig.HttpProtocol);

            feed.Items = items.Select(s => s.GetProductSyndicationItem(url, rssParams));
            var formatter = new Rss20FeedFormatter(feed);
            formatter.SerializeExtensionsAsAtom = false;
            XNamespace atom = "http://www.w3.org/2005/Atom";
            formatter.Feed.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            formatter.Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", atomSelfHref.ToString()), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

            return formatter;
        }

        public async Task<Rss20FeedFormatter> GetProductsRssAsync(RssParams rssParams, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = HttpContextFactory.Create()?.Request;
            var host = request?.Url?.Host ?? "localhost";
            var builder = new UriBuilder(AppConfig.HttpProtocol, host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));
            var urlHelper = request?.RequestContext != null ? new UrlHelper(request.RequestContext) : null;
            String atomSelfHref = urlHelper?.Action("products", "rss", new { rssParams.Take, rssParams.Language }, AppConfig.HttpProtocol)
                ?? $"{url}/rss/products?take={rssParams.Take}&language={rssParams.Language}";

            var items = await ProductRepository.GetActiveProductsForRssAsync(rssParams.Language, rssParams.Take, null, cancellationToken).ConfigureAwait(false);

            String title = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

            feed.Items = items.Select(s => s.GetProductSyndicationItem(url, rssParams));
            var formatter = new Rss20FeedFormatter(feed);
            formatter.SerializeExtensionsAsAtom = false;
            XNamespace atom = "http://www.w3.org/2005/Atom";
            formatter.Feed.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            formatter.Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", atomSelfHref), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

            return formatter;
        }

        public Rss20FeedFormatter GetProductCategoriesRss(RssParams rssParams)
        {
            var productCategory = rssParams.CategoryId > 0
                ? ProductCategoryService.GetSingle(rssParams.CategoryId)
                : null;
            var items = ProductRepository.GetActiveProductsForRss(rssParams.Language, rssParams.Take, rssParams.CategoryId > 0 ? (int?)rssParams.CategoryId : null);
            var request = HttpContextFactory.Create()?.Request;
            var host = request?.Url?.Host ?? "localhost";
            var builder = new UriBuilder(AppConfig.HttpProtocol, host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));

            String companyName = SettingService.GetSettingByKey(Constants.CompanyName);
            String title = productCategory != null ? $"{companyName} - {productCategory.Name}" : companyName;
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, productCategory?.Description ?? "", new Uri(url))
            {
                Language = lang
            };

            var urlHelper = request?.RequestContext != null ? new UrlHelper(request.RequestContext) : null;
            String atomSelfHref = urlHelper?.Action("productcategories", "rss", new { rssParams.Take, rssParams.Language, rssParams.CategoryId }, AppConfig.HttpProtocol)
                ?? $"{url}/rss/productcategories?categoryId={rssParams.CategoryId}&take={rssParams.Take}&language={rssParams.Language}";

            feed.Items = items.Select(s => s.GetProductSyndicationItem(url, rssParams));
            var formatter = new Rss20FeedFormatter(feed);
            formatter.SerializeExtensionsAsAtom = false;
            XNamespace atom = "http://www.w3.org/2005/Atom";
            formatter.Feed.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            formatter.Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", atomSelfHref), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

            return formatter;
        }

        public async Task<Rss20FeedFormatter> GetProductCategoriesRssAsync(RssParams rssParams, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = HttpContextFactory.Create()?.Request;
            var host = request?.Url?.Host ?? "localhost";
            var builder = new UriBuilder(AppConfig.HttpProtocol, host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));
            var urlHelper = request?.RequestContext != null ? new UrlHelper(request.RequestContext) : null;
            String atomSelfHref = urlHelper?.Action("productcategories", "rss", new { rssParams.Take, rssParams.Language, rssParams.CategoryId }, AppConfig.HttpProtocol)
                ?? $"{url}/rss/productcategories?categoryId={rssParams.CategoryId}&take={rssParams.Take}&language={rssParams.Language}";

            var productCategory = rssParams.CategoryId > 0
                ? await ProductCategoryService.GetSingleAsync(rssParams.CategoryId).ConfigureAwait(false)
                : null;
            var items = await ProductRepository.GetActiveProductsForRssAsync(rssParams.Language, rssParams.Take, rssParams.CategoryId > 0 ? (int?)rssParams.CategoryId : null, cancellationToken).ConfigureAwait(false);

            String companyName = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            String title = productCategory != null ? $"{companyName} - {productCategory.Name}" : companyName;
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, productCategory?.Description ?? "", new Uri(url))
            {
                Language = lang
            };

            feed.Items = items.Select(s => s.GetProductSyndicationItem(url, rssParams));
            var formatter = new Rss20FeedFormatter(feed);
            formatter.SerializeExtensionsAsAtom = false;
            XNamespace atom = "http://www.w3.org/2005/Atom";
            formatter.Feed.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            formatter.Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", atomSelfHref), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

            return formatter;
        }

        public ProductsSearchResult GetProductsSearchResult(
         string search,
         string filters,
         string page,
         int language)
        {
            int top = 10;
            int skip = 0;
            return ProductRepository.GetProductsSearchResult(search, filters, top, skip, language);
        }

        public async Task<ProductsSearchResult> GetProductsSearchResultAsync(
         string search,
         string filters,
         string page,
         int language,
         CancellationToken cancellationToken = default(CancellationToken))
        {
            int top = 10;
            int skip = 0;
            return await ProductRepository.GetProductsSearchResultAsync(search, filters, top, skip, language, cancellationToken).ConfigureAwait(false);
        }

        public void ParseTemplateAndSaveProductSpecifications(int productId, int templateId, int currentLanguage, HttpRequestBase request)
        {
            var template = TemplateService.GetTemplate(templateId);
            XDocument xdoc = XDocument.Parse(template.TemplateXml);
            var groups = xdoc.Root.Descendants("group");
            var Specifications = new List<ProductSpecification>();

            foreach (var group in groups)
            {
                var groupName = group.FirstAttribute.Value;
                int position = 1;
                foreach (XElement field in group.Elements())
                {
                    var p = new ProductSpecification();
                    p.GroupName = groupName;
                    p.ProductId = productId;
                    p.CreatedDate = DateTime.Now;
                    p.UpdatedDate = DateTime.Now;
                    p.Position = position++;
                    p.IsActive = true;
                    p.Lang = currentLanguage;
                    var name = field.Attribute("name");
                    var unit = field.Attribute("unit");

                    var value = ReadSpecFormValue(request, field, name != null ? name.Value : null);

                    if (name != null)
                    {
                        p.Name = name.Value;
                    }
                    if (unit != null)
                    {
                        p.Unit = unit.Value;
                    }

                    p.Value = NormalizeSpecFieldValue(field, value);
                    Specifications.Add(p);
                }
            }

            SaveProductSpecifications(Specifications, productId);
        }

        public async Task ParseTemplateAndSaveProductSpecificationsAsync(int productId, int templateId, int currentLanguage, HttpRequestBase request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var template = await TemplateService.GetTemplateAsync(templateId, cancellationToken).ConfigureAwait(false);
            XDocument xdoc = XDocument.Parse(template.TemplateXml);
            var groups = xdoc.Root.Descendants("group");
            var Specifications = new List<ProductSpecification>();

            foreach (var group in groups)
            {
                var groupName = group.FirstAttribute.Value;
                int position = 1;
                foreach (XElement field in group.Elements())
                {
                    var p = new ProductSpecification();
                    p.GroupName = groupName;
                    p.ProductId = productId;
                    p.CreatedDate = DateTime.Now;
                    p.UpdatedDate = DateTime.Now;
                    p.Position = position++;
                    p.IsActive = true;
                    p.Lang = currentLanguage;
                    var name = field.Attribute("name");
                    var unit = field.Attribute("unit");

                    var value = ReadSpecFormValue(request, field, name != null ? name.Value : null);

                    if (name != null)
                    {
                        p.Name = name.Value;
                    }
                    if (unit != null)
                    {
                        p.Unit = unit.Value;
                    }

                    p.Value = NormalizeSpecFieldValue(field, value);
                    Specifications.Add(p);
                }
            }

            await SaveProductSpecificationsAsync(Specifications, productId).ConfigureAwait(false);
        }

        private static string ReadSpecFormValue(HttpRequestBase request, XElement field, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName) || request == null)
            {
                return null;
            }

            if (ProductSpecificationFieldHelper.IsMultiSelectField(field))
            {
                var posted = request.Unvalidated.Form.GetValues(fieldName);
                if (posted != null && posted.Length > 0)
                {
                    return ProductSpecificationFieldHelper.NormalizeMultiSelectStorageValue(posted);
                }

                // Form.Get joins multi-values with commas when GetValues is unavailable.
                return ProductSpecificationFieldHelper.NormalizeMultiSelectStorageValue(
                    request.Unvalidated.Form.Get(fieldName));
            }

            return request.Unvalidated.Form.Get(fieldName);
        }

        private static string NormalizeSpecFieldValue(XElement field, string value)
        {
            if (ProductSpecificationValueHelper.IsCheckboxField(field))
            {
                return ProductSpecificationValueHelper.NormalizeCheckboxStorageValue(value);
            }

            if (ProductSpecificationFieldHelper.IsMultiSelectField(field))
            {
                return ProductSpecificationFieldHelper.NormalizeMultiSelectStorageValue(value);
            }

            if (ProductSpecificationFieldHelper.IsDateTimeField(field))
            {
                return ProductSpecificationFieldHelper.NormalizeDateTimeStorageValue(
                    value,
                    ProductSpecificationFieldHelper.IncludeTime(field));
            }

            return value;
        }

        public void MoveProductsInTrees(int newCategoryId, String products)
        {
            if (!String.IsNullOrEmpty(products))
            {
                var productIdList = products.Split(',');
                foreach (var id in productIdList)
                {
                    var product = ProductRepository.GetProduct(id.ToInt());
                    product.ProductCategoryId = newCategoryId;
                    ProductRepository.Edit(product);
                }
                ProductRepository.Save();
                // Category membership feeds category listings and category product counts.
                InvalidateProductListCaches();
            }
        }

        public async Task MoveProductsInTreesAsync(int newCategoryId, String products, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!String.IsNullOrEmpty(products))
            {
                var productIdList = products.Split(',');
                foreach (var id in productIdList)
                {
                    var product = await ProductRepository.GetProductAsync(id.ToInt(), cancellationToken).ConfigureAwait(false);
                    product.ProductCategoryId = newCategoryId;
                    ProductRepository.Edit(product);
                }
                await ProductRepository.SaveAsync().ConfigureAwait(false);
                InvalidateProductListCaches();
            }
        }

        public Product GetProductById(int id)
        {
            return ProductRepository.GetProduct(id);
        }

        public async Task<Product> GetProductByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetProductAsync(id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Models.DTOs.Storefront.StorefrontProductCardDto> GetStorefrontProductCardByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductRepository.GetStorefrontProductCardByIdAsync(id, cancellationToken).ConfigureAwait(false);
        }

        public List<Product> GetChildrenProducts(ProductCategory productCategory, List<ProductCategory> ChildrenProductCategories)
        {
            if (productCategory == null || ChildrenProductCategories.IsEmpty())
            {
                return new List<Product>();
            }
            var allCategoriesId = new List<int>();
            // GetChildren Category Id s
            int[] childrenCategoryId = ChildrenProductCategories.Select(r => r.Id).ToArray();
            allCategoriesId.AddRange(childrenCategoryId);
            var allActiveCategories = ProductCategoryService.GetActiveBaseContents(true, productCategory.Lang);
            foreach (var category in allActiveCategories)
            {
                foreach (var childrenId in childrenCategoryId)
                {
                    if (category.ParentId == childrenId)
                    {
                        allCategoriesId.Add(category.Id);
                    }
                }
            }
            return ProductRepository.GetChildrenProducts(allCategoriesId.ToArray());
        }

        public async Task<List<Product>> GetChildrenProductsAsync(ProductCategory productCategory, List<ProductCategory> ChildrenProductCategories, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (productCategory == null || ChildrenProductCategories.IsEmpty())
            {
                return new List<Product>();
            }
            var allCategoriesId = new List<int>();
            int[] childrenCategoryId = ChildrenProductCategories.Select(r => r.Id).ToArray();
            allCategoriesId.AddRange(childrenCategoryId);
            var allActiveCategories = await ProductCategoryService.GetActiveBaseContentsFromCacheAsync(true, productCategory.Lang).ConfigureAwait(false);
            foreach (var category in allActiveCategories)
            {
                foreach (var childrenId in childrenCategoryId)
                {
                    if (category.ParentId == childrenId)
                    {
                        allCategoriesId.Add(category.Id);
                    }
                }
            }
            return await ProductRepository.GetChildrenProductsAsync(allCategoriesId.ToArray(), cancellationToken).ConfigureAwait(false);
        }

        public void ApplySoldCounts(IList<Product> products)
        {
            if (products == null || products.Count == 0)
            {
                return;
            }

            var soldQuantities = OrderProductRepository.GetSoldQuantities(products.Select(p => p.Id));
            foreach (var product in products)
            {
                int soldCount;
                product.SoldCount = soldQuantities.TryGetValue(product.Id, out soldCount) ? soldCount : 0;
            }
        }

        [Timed("service.products.get_by_tag_id_simple_sync")]
        public virtual SimiliarProductTagsViewModel GetProductByTagId(int tagId, int pageIndex, int pageSize, int lang)
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = TagService.GetStorefrontTagById(tagId);
            r.Products = ProductRepository.GetStorefrontProductsByTagId(tagId, pageIndex, pageSize, lang, SortingType.Newest);
            r.StoryTags = new PaginatedList<StorefrontStoryCardDto>(new List<StorefrontStoryCardDto>(), 1, 10, 0);
            return r;
        }

        [Timed("service.products.get_by_tag_id_sync")]
        public virtual SimiliarProductTagsViewModel GetProductByTagId(int tagId, int page, int pageSize, int currentLanguage, SortingType sorting)
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = TagService.GetStorefrontTagById(tagId);
            r.Products = ProductRepository.GetStorefrontProductsByTagId(tagId, page, pageSize, currentLanguage, sorting);
            r.StoryTags = new PaginatedList<StorefrontStoryCardDto>(new List<StorefrontStoryCardDto>(), 1, 10, 0);
            return r;
        }

        [Timed("service.products.get_by_tag_id_simple")]
        public virtual async Task<SimiliarProductTagsViewModel> GetProductByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = await TagService.GetStorefrontTagByIdAsync(tagId, cancellationToken).ConfigureAwait(false);
            r.Products = await ProductRepository.GetStorefrontProductsByTagIdAsync(tagId, pageIndex, pageSize, lang, SortingType.Newest, cancellationToken).ConfigureAwait(false);
            r.StoryTags = new PaginatedList<StorefrontStoryCardDto>(new List<StorefrontStoryCardDto>(), 1, 10, 0);
            return r;
        }

        [Timed("service.products.get_by_tag_id")]
        public virtual async Task<SimiliarProductTagsViewModel> GetProductByTagIdAsync(int tagId, int page, int pageSize, int currentLanguage, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken))
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = await TagService.GetStorefrontTagByIdAsync(tagId, cancellationToken).ConfigureAwait(false);
            r.Products = await ProductRepository.GetStorefrontProductsByTagIdAsync(tagId, page, pageSize, currentLanguage, sorting, cancellationToken).ConfigureAwait(false);
            r.StoryTags = new PaginatedList<StorefrontStoryCardDto>(new List<StorefrontStoryCardDto>(), 1, 10, 0);
            return r;
        }

        public void ChangeProductState(List<string> values, ProductState state)
        {
            if (values == null || values.IsEmpty())
            {
                return;
            }
            foreach (var id in values)
            {
                var product = ProductRepository.GetProduct(id.ToInt());
                product.StateEnum = state;
                ProductRepository.Edit(product);
            }
            ProductRepository.Save();
            // Activation/deactivation changes storefront visibility immediately.
            InvalidateProductListCaches();
        }

        public async Task ChangeProductStateAsync(List<string> values, ProductState state)
        {
            if (values == null || values.IsEmpty())
            {
                return;
            }
            foreach (var id in values)
            {
                var product = await ProductRepository.GetProductAsync(id.ToInt()).ConfigureAwait(false);
                product.StateEnum = state;
                ProductRepository.Edit(product);
            }
            await ProductRepository.SaveAsync().ConfigureAwait(false);
            InvalidateProductListCaches();
        }

        public virtual void DecreaseStock(int productId, int quantity)
        {
            if (productId <= 0 || quantity <= 0)
            {
                return;
            }

            var product = ProductRepository.GetProduct(productId);
            if (product == null)
            {
                ProductServiceLogger.Warn($"DecreaseStock: Product with Id {productId} not found.");
                return;
            }

            ProductServiceLogger.Info($"DecreaseStock: ProductId: {productId}, ProductName: {product.Name}, Quantity: {quantity}, State: {product.State}");
            product.UpdatedDate = DateTime.Now;
            ProductRepository.Edit(product);
            ProductRepository.Save();
            InvalidateProductListCaches();
        }

        public virtual async Task DecreaseStockAsync(int productId, int quantity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (productId <= 0 || quantity <= 0)
            {
                return;
            }

            var product = await ProductRepository.GetProductAsync(productId, cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                ProductServiceLogger.Warn($"DecreaseStockAsync: Product with Id {productId} not found.");
                return;
            }

            ProductServiceLogger.Info($"DecreaseStockAsync: ProductId: {productId}, ProductName: {product.Name}, Quantity: {quantity}, State: {product.State}");
            product.UpdatedDate = DateTime.Now;
            ProductRepository.Edit(product);
            await ProductRepository.SaveAsync().ConfigureAwait(false);
            InvalidateProductListCaches();
        }
    }
}