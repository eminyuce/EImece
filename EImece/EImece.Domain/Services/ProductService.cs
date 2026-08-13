using EImece.Domain.Caching;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
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
            if (request == null || request.PercentageOfIncreaseOrDecrease == null)
            {
                return "hata";
            }
            var connectionString = this.ProductRepository.GetDbContext().Database.Connection.ConnectionString;
            var commandText = @"[dbo].[UpdateProductPrices]";
            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter("PercentageOfIncreaseOrDecrease", request.PercentageOfIncreaseOrDecrease, SqlDbType.Decimal));
            parameterList.Add(DatabaseUtility.GetSqlParameter("ProductId", (object)request.ProductId ?? DBNull.Value, SqlDbType.Int));
            parameterList.Add(DatabaseUtility.GetSqlParameter("CategoryId", (object)request.CategoryId ?? DBNull.Value, SqlDbType.Int));
            parameterList.Add(DatabaseUtility.GetSqlParameter("BrandId", (object)request.BrandId ?? DBNull.Value, SqlDbType.Int));
            parameterList.Add(DatabaseUtility.GetSqlParameter("TagId", (object)request.TagId ?? DBNull.Value, SqlDbType.Int));
            var commandType = CommandType.StoredProcedure;
            var result = DatabaseUtility.ExecuteScalar(new SqlConnection(connectionString), commandText, commandType, parameterList.ToArray()).ToStr();
            InvalidateProductListCaches();
            return result;
        }

        public async Task<string> UpdatePricesAsync(UpdatePriceRequest request)
        {
            if (request == null || request.PercentageOfIncreaseOrDecrease == null)
            {
                return "hata";
            }
            var connectionString = this.ProductRepository.GetDbContext().Database.Connection.ConnectionString;
            var commandText = @"[dbo].[UpdateProductPrices]";
            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter("PercentageOfIncreaseOrDecrease", request.PercentageOfIncreaseOrDecrease, SqlDbType.Decimal));
            parameterList.Add(DatabaseUtility.GetSqlParameter("ProductId", (object)request.ProductId ?? DBNull.Value, SqlDbType.Int));
            parameterList.Add(DatabaseUtility.GetSqlParameter("CategoryId", (object)request.CategoryId ?? DBNull.Value, SqlDbType.Int));
            parameterList.Add(DatabaseUtility.GetSqlParameter("BrandId", (object)request.BrandId ?? DBNull.Value, SqlDbType.Int));
            parameterList.Add(DatabaseUtility.GetSqlParameter("TagId", (object)request.TagId ?? DBNull.Value, SqlDbType.Int));
            var commandType = CommandType.StoredProcedure;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = commandType;
                    command.Parameters.AddRange(parameterList.ToArray());
                    var scalar = await command.ExecuteScalarAsync().ConfigureAwait(false);
                    InvalidateProductListCaches();
                    return scalar.ToStr();
                }
            }
        }

        public ProductIndexViewModel GetMainPageProducts(int pageIndex, int lang)
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
                    result.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName);
                    result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, lang).FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
                    result.ProductMenu = MenuService.GetActiveBaseContentsFromCache(true, lang).FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));

                    var items = ProductRepository.GetActiveProducts(pageIndex, pageSize, lang);
                    result.Products = items;
                    result.Tags = TagService.GetActiveBaseEntities(true, lang);
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
        public async Task<ProductIndexViewModel> GetMainPageProductsAsync(int pageIndex, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new ProductIndexViewModel();
            int pageSize = AppConfig.RecordPerPage;

            result.CompanyName = await SettingService.GetSettingObjectByKeyAsync(Constants.CompanyName).ConfigureAwait(false);

            var menus = await MenuService.GetActiveBaseContentsFromCacheAsync(true, lang).ConfigureAwait(false);
            result.MainPageMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.ProductMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));

            result.Products = await ProductRepository.GetActiveProductsAsync(pageIndex, pageSize, lang, cancellationToken).ConfigureAwait(false);
            result.Tags = await TagService.GetActiveBaseEntitiesFromCacheAsync(true, lang).ConfigureAwait(false);

            return result;
        }

        public void SaveProductTags(int id, int[] tags)
        {
            ProductTagRepository.SaveProductTags(id, tags);
        }

        public async Task SaveProductTagsAsync(int id, int[] tags)
        {
            await ProductTagRepository.SaveProductTagsAsync(id, tags).ConfigureAwait(false);
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

        public ProductDetailViewModel GetProductDetailViewModelById(int id)
        {
            var result = new ProductDetailViewModel();
            var product = ProductRepository.GetProduct(id);

            if (product == null)
            {
                return null;
            }
            if (!product.IsActive)
            {
                result.Product = product;
                return result;
            }
            result.IsProductPriceEnable = SettingService.GetSettingObjectByKey(Constants.IsProductPriceEnable);
            result.IsProductReviewEnable = SettingService.GetSettingObjectByKey(Constants.IsProductReviewEnable);
            result.PaymentDetailHtml = SettingService.GetSettingObjectByKey(Constants.PaymentDetailHtml);
            result.WhatsAppCommunicationLink = SettingService.GetSettingObjectByKey(Constants.WhatsAppCommunicationLink);
            result.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName);
            // if (product.MainImageId.HasValue)
            // {
            //     FileStorage fileStorage = null;
            //     product.MainImageBytes = FilesHelper.GetFileStorageFromCache(product.MainImageId.Value, out fileStorage);
            // }
            if (product.MainImageId.HasValue)
            {
                product.MainImageSrc = FilesHelper.GetImageSrcPath(product.MainImageId.Value);
            }
            else
            {
                product.MainImageSrc = new Tuple<string, string>("", "");
            }
            result.Contact = ContactUsFormViewModel.CreateContactUsFormViewModel("productDetail", id, EImeceItemType.Product);
            product.ProductComments = EntityFilterHelper.FilterProductComments(product.ProductComments);
            result.CargoDescription = SettingService.GetSettingObjectByKey(Constants.CargoDescription, product.Lang);
            result.CargoPrice = SettingService.GetSettingObjectByKey(Constants.CargoPrice, product.Lang);
            List<Menu> menuList = MenuService.GetActiveBaseContentsFromCache(true, product.Lang);
            result.MainPageMenu = menuList.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.ProductMenu = menuList.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.SocialMediaLinks = SettingService.CreateShareableSocialMediaLinks(product.DetailPageAbsoluteUrl, product.NameLong, product.ImageFullPath(1000, 0));
            result.Product = product;
            EntityFilterHelper.FilterProduct(result.Product);
            if (product.ProductCategory.TemplateId.HasValue)
            {
                result.Template = TemplateService.GetTemplate(product.ProductCategory.TemplateId.Value);
            }
            result.BreadCrumb = ProductCategoryService.GetBreadCrumb(product.ProductCategoryId, product.Lang);
            result.RelatedStories = new List<Story>();
            // if (product.ProductTags.Any())
            // {
            //    var tagIdList = product.ProductTags.Select(t => t.TagId).ToArray();
            // result.RelatedStories = StoryRepository.GetRelatedStories(tagIdList, 20, product.Lang, 0);
            // }
            int relatedProductTake = 20;
            result.RelatedProducts = new List<Product>();
            if (product.ProductTags.Any())
            {
                var tagIdList = product.ProductTags.Select(t => t.TagId).ToArray();
                result.RelatedProducts = this.GetRelatedProducts(tagIdList, relatedProductTake, product.Lang, id);
            }

            if (result.RelatedProducts.Count < 20)
            {
                relatedProductTake -= result.RelatedProducts.Count;
                result.RelatedProducts.AddRange(this.GetRandomProductsByCategoryId(product.ProductCategoryId, relatedProductTake, product.Lang, id));
            }

            result.RelatedProducts = result.RelatedProducts.Distinct().OrderByStorefrontDefault().Take(20).ToList();

            return result;
        }

        public async Task<ProductDetailViewModel> GetProductDetailViewModelByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new ProductDetailViewModel();
            var product = await ProductRepository.GetProductAsync(id, cancellationToken).ConfigureAwait(false);

            if (product == null)
            {
                return null;
            }
            if (!product.IsActive)
            {
                result.Product = product;
                return result;
            }
            result.IsProductPriceEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductPriceEnable).ConfigureAwait(false);
            result.IsProductReviewEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductReviewEnable).ConfigureAwait(false);
            result.PaymentDetailHtml = await SettingService.GetSettingObjectByKeyAsync(Constants.PaymentDetailHtml).ConfigureAwait(false);
            result.WhatsAppCommunicationLink = await SettingService.GetSettingObjectByKeyAsync(Constants.WhatsAppCommunicationLink).ConfigureAwait(false);
            result.CompanyName = await SettingService.GetSettingObjectByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            if (product.MainImageId.HasValue)
            {
                product.MainImageSrc = FilesHelper.GetImageSrcPath(product.MainImageId.Value);
            }
            else
            {
                product.MainImageSrc = new Tuple<string, string>("", "");
            }
            result.Contact = ContactUsFormViewModel.CreateContactUsFormViewModel("productDetail", id, EImeceItemType.Product);
            product.ProductComments = EntityFilterHelper.FilterProductComments(product.ProductComments);
            result.CargoDescription = await SettingService.GetSettingObjectByKeyAsync(Constants.CargoDescription, product.Lang).ConfigureAwait(false);
            result.CargoPrice = await SettingService.GetSettingObjectByKeyAsync(Constants.CargoPrice, product.Lang).ConfigureAwait(false);
            List<Menu> menuList = await MenuService.GetActiveBaseContentsFromCacheAsync(true, product.Lang).ConfigureAwait(false);
            result.MainPageMenu = menuList.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.ProductMenu = menuList.FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            result.SocialMediaLinks = SettingService.CreateShareableSocialMediaLinks(product.DetailPageAbsoluteUrl, product.NameLong, product.ImageFullPath(1000, 0));
            result.Product = product;
            EntityFilterHelper.FilterProduct(result.Product);
            if (product.ProductCategory != null && product.ProductCategory.TemplateId.HasValue)
            {
                result.Template = await TemplateService.GetTemplateAsync(product.ProductCategory.TemplateId.Value, cancellationToken).ConfigureAwait(false);
            }
            result.BreadCrumb = await ProductCategoryService.GetBreadCrumbAsync(product.ProductCategoryId, product.Lang).ConfigureAwait(false);
            result.RelatedStories = new List<Story>();
            int relatedProductTake = 20;
            result.RelatedProducts = new List<Product>();
            if (product.ProductTags != null && product.ProductTags.Any())
            {
                var tagIdList = product.ProductTags.Select(t => t.TagId).ToArray();
                result.RelatedProducts = await GetRelatedProductsAsync(tagIdList, relatedProductTake, product.Lang, id, cancellationToken).ConfigureAwait(false);
            }

            if (result.RelatedProducts.Count < 20)
            {
                relatedProductTake -= result.RelatedProducts.Count;
                result.RelatedProducts.AddRange(await GetRandomProductsByCategoryIdAsync(product.ProductCategoryId, relatedProductTake, product.Lang, id, cancellationToken).ConfigureAwait(false));
            }

            result.RelatedProducts = result.RelatedProducts.Distinct().OrderByStorefrontDefault().Take(20).ToList();

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
            var isAnyProductSold = OrderProductRepository.FindBy(r => r.ProductId == id).Any();
            if (isAnyProductSold)
            {
                ProductServiceLogger.Info("Product cannot be deleted because it has order history. ProductId: " + id);
                return ProductDeleteResult.BlockedByOrders;
            }

            try
            {
                var product = ProductRepository.GetProduct(id);
                if (product == null)
                {
                    return ProductDeleteResult.Failed;
                }

                ProductCommentRepository.DeleteByWhereCondition(r => r.ProductId == id);
                ProductSpecificationRepository.DeleteByWhereCondition(r => r.ProductId == id);
                ProductTagRepository.DeleteByWhereCondition(r => r.ProductId == id);
                FileStorageService.DeleteGalleryImages(id, MediaModType.Products);
                if (product.MainImageId.HasValue)
                {
                    FileStorageService.DeleteFileStorage(product.MainImageId.Value);
                }
                DeleteEntity(product);
                InvalidateProductListCaches();
                return ProductDeleteResult.Deleted;
            }
            catch (Exception e)
            {
                ProductServiceLogger.Error(e, "DeleteProductById did not work for productId:" + id);
                return ProductDeleteResult.Failed;
            }
        }

        public async Task<ProductDeleteResult> DeleteProductByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var isAnyProductSold = await OrderProductRepository.FindBy(r => r.ProductId == id).AnyAsync(cancellationToken).ConfigureAwait(false);
            if (isAnyProductSold)
            {
                ProductServiceLogger.Info("Product cannot be deleted because it has order history. ProductId: " + id);
                return ProductDeleteResult.BlockedByOrders;
            }

            try
            {
                var product = await ProductRepository.GetProductAsync(id, cancellationToken).ConfigureAwait(false);
                if (product == null)
                {
                    return ProductDeleteResult.Failed;
                }

                await ProductCommentRepository.DeleteByWhereConditionAsync(r => r.ProductId == id).ConfigureAwait(false);
                await ProductSpecificationRepository.DeleteByWhereConditionAsync(r => r.ProductId == id).ConfigureAwait(false);
                await ProductTagRepository.DeleteByWhereConditionAsync(r => r.ProductId == id).ConfigureAwait(false);
                await FileStorageService.DeleteGalleryImagesAsync(id, MediaModType.Products).ConfigureAwait(false);
                if (product.MainImageId.HasValue)
                {
                    await FileStorageService.DeleteFileStorageAsync(product.MainImageId.Value).ConfigureAwait(false);
                }
                await DeleteEntityAsync(product).ConfigureAwait(false);
                InvalidateProductListCaches();
                return ProductDeleteResult.Deleted;
            }
            catch (Exception e)
            {
                ProductServiceLogger.Error(e, "DeleteProductById did not work for productId:" + id);
                return ProductDeleteResult.Failed;
            }
        }

        public ProductsSearchViewModel SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting)
        {
            // Absolute + short TTL: search cardinality is high; keep entries brief and invalidate
            // the whole search prefix when any product changes (see InvalidateProductListCaches).
            var cacheKey = CacheKeys.ProductSearch(search, pageIndex, pageSize, lang, sorting);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => BuildProductsSearchViewModel(pageIndex, pageSize, search, lang, sorting),
                CachePolicy.Absolute(AppConfig.CacheShortSeconds));
        }

        public async Task<ProductsSearchViewModel> SearchProductsAsync(int pageIndex, int pageSize, string search, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken))
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
                r.Products = ProductRepository.SearchProducts(pageIndex, pageSize, search, lang, sorting);
            }
            else
            {
                r.Products = new PaginatedList<Product>(new List<Product>(), pageIndex, pageSize, 0);
            }

            r.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, lang).FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.HomeIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));
            r.ProductMenu = MenuService.GetActiveBaseContentsFromCache(true, lang).FirstOrDefault(r1 => r1.MenuLink.Equals(Constants.ProductsIndexMenuLink, StringComparison.InvariantCultureIgnoreCase));

            return r;
        }

        private async Task<ProductsSearchViewModel> BuildProductsSearchViewModelAsync(int pageIndex, int pageSize, string search, int lang, SortingType sorting, CancellationToken cancellationToken)
        {
            var r = new ProductsSearchViewModel();
            r.Search = search;
            if (!String.IsNullOrEmpty(search))
            {
                r.Products = await ProductRepository.SearchProductsAsync(pageIndex, pageSize, search, lang, sorting, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                r.Products = new PaginatedList<Product>(new List<Product>(), pageIndex, pageSize, 0);
            }

            var menus = await MenuService.GetActiveBaseContentsFromCacheAsync(true, lang).ConfigureAwait(false);
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
        /// Drops every product list/search MemoryCache entry after a mutating admin action so the
        /// next storefront request rebuilds from SQL instead of serving stale AbsoluteExpiration data.
        /// </summary>
        public void InvalidateProductListCaches()
        {
            var listRemoved = DataCachingProvider.ClearByPrefix(CacheKeys.ProductListPrefix);
            var searchRemoved = DataCachingProvider.ClearByPrefix(CacheKeys.ProductSearchPrefix);
            ProductServiceLogger.Info(
                "InvalidateProductListCaches removed {0} list + {1} search entries",
                listRemoved,
                searchRemoved);
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
            var items = this.GetActiveProducts(rssParams.Language).Take(rssParams.Take).ToList();
            // FIX: use the injected IHttpContextFactory abstraction instead of the static
            // System.Web.HttpContext.Current ambient (testable; removes the hard web coupling).
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
            var items = (await GetActiveProductsAsync(rssParams.Language, cancellationToken).ConfigureAwait(false)).Take(rssParams.Take).ToList();
            var request = HttpContextFactory.Create().Request;
            var builder = new UriBuilder(AppConfig.HttpProtocol, request.Url.Host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));

            String title = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
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

        public SimiliarProductTagsViewModel GetProductByTagId(int tagId, int pageIndex, int pageSize, int lang)
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = TagService.GetSingle(tagId);
            r.ProductTags = ProductTagRepository.GetProductsByTagId(tagId, pageIndex, pageSize, lang);
            r.StoryTags = StoryTagRepository.GetStoriesByTagId(tagId, 1, 10, lang);
            return r;
        }

        public SimiliarProductTagsViewModel GetProductByTagId(int tagId, int page, int pageSize, int currentLanguage, SortingType sorting)
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = TagService.GetSingle(tagId);
            r.ProductTags = ProductTagRepository.GetProductsByTagId(tagId, page, pageSize, currentLanguage, sorting);
            r.StoryTags = StoryTagRepository.GetStoriesByTagId(tagId, 1, 10, currentLanguage);
            return r;
        }

        public async Task<SimiliarProductTagsViewModel> GetProductByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = await TagService.GetSingleAsync(tagId).ConfigureAwait(false);
            r.ProductTags = await ProductTagRepository.GetProductsByTagIdAsync(tagId, pageIndex, pageSize, lang, cancellationToken).ConfigureAwait(false);
            r.StoryTags = await StoryTagRepository.GetStoriesByTagIdAsync(tagId, 1, 10, lang, cancellationToken).ConfigureAwait(false);
            return r;
        }

        public async Task<SimiliarProductTagsViewModel> GetProductByTagIdAsync(int tagId, int page, int pageSize, int currentLanguage, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken))
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = await TagService.GetSingleAsync(tagId).ConfigureAwait(false);
            r.ProductTags = await ProductTagRepository.GetProductsByTagIdAsync(tagId, page, pageSize, currentLanguage, sorting, cancellationToken).ConfigureAwait(false);
            r.StoryTags = await StoryTagRepository.GetStoriesByTagIdAsync(tagId, 1, 10, currentLanguage, cancellationToken).ConfigureAwait(false);
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
        }
    }
}