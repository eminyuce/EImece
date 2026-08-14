using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class ProductRepository : BaseContentRepository<Product>, IProductRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ProductRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public PaginatedList<Product> GetActiveProducts(int pageIndex, int pageSize, int language)
        {
            try
            {
                Expression<Func<Product, object>> includeProperty1 = r => r.ProductFiles;

                Expression<Func<Product, object>> includeProperty2 = r => r.ProductCategory;
                Expression<Func<Product, object>> includeProperty3 = r => r.MainImage;
                Expression<Func<Product, object>> includeProperty4 = r => r.ProductTags.Select(t => t.Tag);
                Expression<Func<Product, object>>[] includeProperties = {
                    includeProperty1,
                    includeProperty2,
                    includeProperty4,

                    includeProperty3 };
                Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == language;
                return GetAllIncludingReadOnly(includeProperties)
                    .Where(match)
                    .OrderByStorefrontDefault()
                    .ToPaginatedList(pageIndex, pageSize);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, exception.Message);
                throw;
            }
        }

        public async Task<PaginatedList<Product>> GetActiveProductsAsync(int pageIndex, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                Expression<Func<Product, object>> includeProperty1 = r => r.ProductFiles;
                Expression<Func<Product, object>> includeProperty2 = r => r.ProductCategory;
                Expression<Func<Product, object>> includeProperty3 = r => r.MainImage;
                Expression<Func<Product, object>> includeProperty4 = r => r.ProductTags.Select(t => t.Tag);
                Expression<Func<Product, object>>[] includeProperties = {
                    includeProperty1,
                    includeProperty2,
                    includeProperty4,
                    includeProperty3 };
                Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == language;

                return await GetAllIncludingReadOnly(includeProperties)
                    .Where(match)
                    .OrderByStorefrontDefault()
                    .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "GetActiveProductsAsync failed.");
                throw new InvalidOperationException("GetActiveProductsAsync failed.", exception);
            }
        }

        public async Task<List<Product>> GetActiveProductsAsync(int? language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.Brand);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == language && r2.ProductCategory.IsActive;
            return await GetAllIncludingReadOnly(includeProperties.ToArray())
                .Where(match)
                .OrderByStorefrontDefault()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public PaginatedList<Product> GetMainPageProducts(int pageIndex, int pageSize, int language)
        {
            try
            {
                Expression<Func<Product, object>> includeProperty1 = r => r.ProductFiles;
                Expression<Func<Product, object>> includeProperty2 = r => r.ProductCategory;
                Expression<Func<Product, object>> includeProperty3 = r => r.MainImage;
                Expression<Func<Product, object>> includeProperty4 = r => r.ProductTags.Select(t => t.Tag);
                Expression<Func<Product, object>>[] includeProperties = { includeProperty1, includeProperty2, includeProperty4, includeProperty3 };
                var inStock = ProductState.ProductInStock.ToString();
                Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.MainPage && r2.Lang == language
                    && r2.State == inStock && r2.Price > 0;
                return GetAllIncludingReadOnly(includeProperties)
                    .Where(match)
                    .OrderByStorefrontDefault()
                    .ToPaginatedList(pageIndex, pageSize);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "GetMainPageProducts failed.");
                throw new InvalidOperationException("GetMainPageProducts failed.", exception);
            }
        }

        public List<Product> GetAdminPageList(int categoryId, string search, int language)
        {
            return GetAdminPageList(categoryId, 0, search, language, null);
        }

        public List<Product> GetAdminPageList(int categoryId, int brandId, string search, int language)
        {
            return GetAdminPageList(categoryId, brandId, search, language, null);
        }

        public List<Product> GetAdminPageList(int categoryId, int brandId, string search, int language, ProductAdminListFilter filter)
        {
            var products = BuildAdminPageListQuery(categoryId, brandId, search, language, filter);
            return products.ToList();
        }

        public async Task<List<Product>> GetAdminPageListAsync(int categoryId, string search, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAdminPageListAsync(categoryId, -1, search, language, null, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Product>> GetAdminPageListAsync(int categoryId, int brandId, string search, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAdminPageListAsync(categoryId, brandId, search, language, null, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Product>> GetAdminPageListAsync(int categoryId, int brandId, string search, int language, ProductAdminListFilter filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            var products = BuildAdminPageListQuery(categoryId, brandId, search, language, filter);
            return await products.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        private IQueryable<Product> BuildAdminPageListQuery(int categoryId, int brandId, string search, int language, ProductAdminListFilter filter)
        {
            Expression<Func<Product, object>> includeProperty4 = r => r.ProductComments;
            Expression<Func<Product, object>> includeProperty3 = r => r.MainImage;
            Expression<Func<Product, object>> includeProperty5 = r => r.Brand;
            Expression<Func<Product, object>> includeProperty2 = r => r.ProductCategory;
            Expression<Func<Product, object>>[] includeProperties = { includeProperty2, includeProperty3, includeProperty4, includeProperty5 };
            var products = GetAllIncluding(includeProperties).Where(r => r.Lang == language);
            search = search.ToStr().Trim();
            if (!String.IsNullOrEmpty(search))
            {
                var productId = search.ToInt();
                if (productId > 0)
                {
                    products = products.Where(r => r.Id == productId);
                }
                else
                {
                    Expression<Func<Product, bool>> whereLamba = r => r.Name.Contains(search)
                    || r.ProductCode.Contains(search)
                          || r.NameLong.Contains(search)
                           || r.NameShort.Contains(search)
                    || r.ProductCategory.Name.Contains(search);
                    products = products.Where(whereLamba);
                }
            }

            if (brandId > 0)
            {
                products = products.Where(r => r.BrandId == brandId);
            }

            if (filter != null)
            {
                products = ApplyAdminListFilters(products, filter);
            }

            if (categoryId > 0)
            {
                products = products.Where(r => r.ProductCategoryId == categoryId);
            }
            else
            {
                // CategoryId is -1 for excel exporting.
                // Skip Take(1000) when advanced filters are active so filtered results are complete.
                var hasAdvancedFilter = filter != null && filter.HasAnyFilter;
                if (String.IsNullOrEmpty(search) && categoryId != -1 && !hasAdvancedFilter && brandId <= 0)
                {
                    products = products.Take(1000);
                }
            }
            products = products.OrderByStorefrontDefault();

            return products;
        }

        private static IQueryable<Product> ApplyAdminListFilters(IQueryable<Product> products, ProductAdminListFilter filter)
        {
            if (!String.IsNullOrWhiteSpace(filter.State))
            {
                var state = filter.State.Trim();
                products = products.Where(r => r.State == state);
            }
            if (filter.IsActive.HasValue)
            {
                var isActive = filter.IsActive.Value;
                products = products.Where(r => r.IsActive == isActive);
            }
            if (filter.MainPage.HasValue)
            {
                var mainPage = filter.MainPage.Value;
                products = products.Where(r => r.MainPage == mainPage);
            }
            if (filter.IsCampaign.HasValue)
            {
                var isCampaign = filter.IsCampaign.Value;
                products = products.Where(r => r.IsCampaign == isCampaign);
            }
            if (filter.ApplyPriceFilter)
            {
                if (filter.MinPrice.HasValue)
                {
                    var minPrice = filter.MinPrice.Value;
                    products = products.Where(r => r.Price >= minPrice);
                }
                if (filter.MaxPrice.HasValue)
                {
                    var maxPrice = filter.MaxPrice.Value;
                    products = products.Where(r => r.Price <= maxPrice);
                }
            }

            return products;
        }

        public Product GetProduct(int id)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductComments);
            includeProperties.Add(r => r.Brand);
            includeProperties.Add(r => r.ProductFiles.Select(q => q.FileStorage));
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.ProductTags.Select(q => q.Tag).Select(q1 => q1.TagCategory));
            includeProperties.Add(r => r.ProductSpecifications);
            var item = GetSingleIncluding(id, includeProperties.ToArray());

            return item;
        }

        public async Task<Product> GetProductAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductComments);
            includeProperties.Add(r => r.Brand);
            includeProperties.Add(r => r.ProductFiles.Select(q => q.FileStorage));
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.ProductTags.Select(q => q.Tag).Select(q1 => q1.TagCategory));
            includeProperties.Add(r => r.ProductSpecifications);
            return await GetSingleIncludingAsync(id, cancellationToken, includeProperties.ToArray()).ConfigureAwait(false);
        }

        public PaginatedList<Product> SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting)
        {
            // Eager-load category/image/tags in one round-trip (Paginate uses AsNoTracking) so the
            // view never triggers N+1 lazy loads under concurrent search traffic.
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.ProductTags.Select(q => q.Tag));

            // Trim once in CLR — embedding search.Trim() inside the expression tree makes EF emit
            // LTRIM/RTRIM (or client evaluation) per predicate and can block index seeks on Name*.
            var term = (search ?? string.Empty).Trim();
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == lang
            && (r2.Name.Contains(term)
            || r2.NameLong.Contains(term)
            || r2.NameShort.Contains(term));

            var query = GetAllIncludingReadOnly(includeProperties.ToArray()).Where(match);
            if (sorting == SortingType.LowHighPrice)
            {
                return query.OrderBy(t => t.Price).ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate)
                    .ToPaginatedList(pageIndex, pageSize);
            }
            else if (sorting == SortingType.HighLowPrice)
            {
                return query.OrderByDescending(t => t.Price).ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate)
                    .ToPaginatedList(pageIndex, pageSize);
            }
            else if (sorting == SortingType.Newest)
            {
                return query.OrderByDescending(t => t.UpdatedDate).ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign)
                    .ToPaginatedList(pageIndex, pageSize);
            }
            else
            {
                return query.OrderByStorefrontDefault().ToPaginatedList(pageIndex, pageSize);
            }
        }

        public async Task<PaginatedList<Product>> SearchProductsAsync(int pageIndex, int pageSize, string search, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.ProductTags.Select(q => q.Tag));

            var term = (search ?? string.Empty).Trim();
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == lang
            && (r2.Name.Contains(term)
            || r2.NameLong.Contains(term)
            || r2.NameShort.Contains(term));

            var query = GetAllIncludingReadOnly(includeProperties.ToArray()).Where(match);
            if (sorting == SortingType.LowHighPrice)
            {
                return await query.OrderBy(t => t.Price).ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate)
                    .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
            }
            else if (sorting == SortingType.HighLowPrice)
            {
                return await query.OrderByDescending(t => t.Price).ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate)
                    .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
            }
            else if (sorting == SortingType.Newest)
            {
                return await query.OrderByDescending(t => t.UpdatedDate).ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign)
                    .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await query.OrderByStorefrontDefault()
                    .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
            }
        }

        public IEnumerable<Product> GetData(out int totalRecords,
            string globalSearch,
            String name,
            int? limitOffset,
            int? limitRowCount,
            string orderBy,
            bool desc)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductCategory);
            var query = GetAllIncluding(includeProperties.ToArray());

            if (!String.IsNullOrWhiteSpace(name))
            {
                query = query.Where(p => p.Name.Contains(name.ToLower()));
            }

            if (!String.IsNullOrWhiteSpace(globalSearch))
            {
                query = query.Where(p => (p.Name).Contains(globalSearch) || (p.ProductCode).Contains(globalSearch));
            }

            totalRecords = query.Count();

            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                switch (orderBy.ToLower())
                {
                    case "firstname":
                        if (!desc)
                            query = query.OrderBy(p => p.Name);
                        else
                            query = query.OrderByDescending(p => p.Name);
                        break;

                    case "lastname":
                        if (!desc)
                            query = query.OrderBy(p => p.ProductCode);
                        else
                            query = query.OrderByDescending(p => p.ProductCode);
                        break;

                    case "id":
                        if (!desc)
                            query = query.OrderBy(p => p.Id);
                        else
                            query = query.OrderByDescending(p => p.Id);
                        break;
                }
            }

            if (limitOffset.HasValue)
            {
                query = query.Skip(limitOffset.Value).Take(limitRowCount.Value);
            }

            return query.ToList();
        }

        public List<Product> GetRelatedProducts(int[] tagIdList, int take, int lang, int excludedProductId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == lang
            && r2.ProductTags.Any(t => tagIdList.Contains(t.TagId))
            && r2.Id != excludedProductId;
            var result = GetAllIncludingReadOnly(includeProperties.ToArray())
                .Where(match)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToList();
            return result.Distinct().ToList();
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int[] tagIdList, int take, int lang, int excludedProductId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == lang
            && r2.ProductTags.Any(t => tagIdList.Contains(t.TagId))
            && r2.Id != excludedProductId;
            var result = await GetAllIncludingReadOnly(includeProperties.ToArray())
                .Where(match)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return result.Distinct().ToList();
        }

        public List<Product> GetRandomProductsByCategoryId(int productCategoryId, int take, int lang, int excludedProductId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == lang && r2.ProductCategoryId == productCategoryId && r2.Id != excludedProductId;
            var result = GetAllIncludingReadOnly(includeProperties.ToArray())
                .Where(match)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToList();
            return result.Distinct().ToList();
        }

        public async Task<List<Product>> GetRandomProductsByCategoryIdAsync(int productCategoryId, int take, int lang, int excludedProductId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == lang && r2.ProductCategoryId == productCategoryId && r2.Id != excludedProductId;
            var result = await GetAllIncludingReadOnly(includeProperties.ToArray())
                .Where(match)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return result.Distinct().ToList();
        }

        public List<Product> GetActiveProducts(int? language)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.Brand);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == language && r2.ProductCategory.IsActive;
            // AsNoTracking: storefront/RSS readers never mutate these graphs; skipping the change
            // tracker cuts allocations under high concurrency.
            return GetAllIncludingReadOnly(includeProperties.ToArray())
                .Where(match)
                .OrderByStorefrontDefault()
                .ToList();
        }

        public List<Product> GetActiveProductsForRss(int language, int take)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.Brand);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == language && r2.ProductCategory.IsActive;
            return GetAllIncludingReadOnly(includeProperties.ToArray())
                .Where(match)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToList();
        }

        public async Task<List<Product>> GetActiveProductsForRssAsync(int language, int take, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.Brand);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == language && r2.ProductCategory.IsActive;
            return await GetAllIncludingReadOnly(includeProperties.ToArray())
                .Where(match)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public static ItemType ProductsItem
        {
            get
            {
                return new ItemType()
                {
                    Name = "Products/Products Directory",
                    Type = typeof(Product),
                    SearchAction = "Index",
                    Controller = "Products",
                    ItemTypeID = 1
                };
            }
        }

        public ProductsSearchResult GetProductsSearchResult(
          string search,
          string filters,
          int top,
          int skip,
          int language)
        {
            var fltrs = FilterHelper.ParseFiltersFromString(filters);

            return GetProductsSearchResult(search, fltrs, top, skip, language);
        }

        public async Task<ProductsSearchResult> GetProductsSearchResultAsync(
          string search,
          string filters,
          int top,
          int skip,
          int language,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            var fltrs = FilterHelper.ParseFiltersFromString(filters);

            return await GetProductsSearchResultAsync(search, fltrs, top, skip, language, cancellationToken).ConfigureAwait(false);
        }

        private ProductsSearchResult GetProductsSearchResult(
           string search,
           List<Filter> filters,
           int top,
           int skip,
           int language)
        {
            var searchResult = new ProductsSearchResult();

            var dtFilters = new DataTable("med_tpt_Filter");

            dtFilters.Columns.Add(Constants.FieldNameColumn);
            dtFilters.Columns.Add(Constants.ValueFirstColumn);
            dtFilters.Columns.Add(Constants.ValueLastColumn);

            if (filters != null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    DataRow dr = dtFilters.NewRow();
                    dr[Constants.FieldNameColumn] = filter.FieldName;
                    dr[Constants.ValueFirstColumn] = filter.ValueFirst;
                    dr[Constants.ValueLastColumn] = filter.ValueLast;
                    dtFilters.Rows.Add(dr);
                }
            }
            var db = this.EImeceDbContext;
            // If using Code First we need to make sure the model is built before we open the connection
            // This isn't required for models created with the EF Designer
            // db.Database.Initialize(force: false);
            var connection = db.Database.Connection;
            try
            {
                connection.Open();

                // Create a SQL command to execute the sproc
                SqlCommand cmd = (SqlCommand)connection.CreateCommand();
                cmd.CommandText = @"test_SearchProducts";
                cmd.CommandType = CommandType.StoredProcedure;
                var parameterList = new List<SqlParameter>();
                parameterList.Add(DatabaseUtility.GetSqlParameter("search", search.ToStr(), SqlDbType.NVarChar));
                parameterList.Add(DatabaseUtility.GetSqlParameter("filter", dtFilters, SqlDbType.Structured));
                parameterList.Add(DatabaseUtility.GetSqlParameter("top", top, SqlDbType.Int));
                parameterList.Add(DatabaseUtility.GetSqlParameter("skip", skip, SqlDbType.Int));
                parameterList.Add(DatabaseUtility.GetSqlParameter("language", language, SqlDbType.Int));

                cmd.Parameters.AddRange(parameterList.ToArray());
                // Run the sproc
                var reader = cmd.ExecuteReader();

                // Read Blogs from the first result set
                var products = ((IObjectContextAdapter)db)
                    .ObjectContext
                    .Translate<Product>(reader, "Products", MergeOption.AppendOnly);

                searchResult.Products = products.OrderByStorefrontDefault().ToList();

                // Move to second result set and read Posts
                reader.NextResult();
                var productCategories = ((IObjectContextAdapter)db)
                    .ObjectContext
                    .Translate<ProductCategory>(reader, "ProductCategories", MergeOption.AppendOnly);

                searchResult.ProductCategories = productCategories.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
            finally
            {
                connection.Close();
            }

            searchResult.PageSize = top;
            return searchResult;
        }

        private async Task<ProductsSearchResult> GetProductsSearchResultAsync(
           string search,
           List<Filter> filters,
           int top,
           int skip,
           int language,
           CancellationToken cancellationToken)
        {
            var searchResult = new ProductsSearchResult();

            var dtFilters = new DataTable("med_tpt_Filter");

            dtFilters.Columns.Add(Constants.FieldNameColumn);
            dtFilters.Columns.Add(Constants.ValueFirstColumn);
            dtFilters.Columns.Add(Constants.ValueLastColumn);

            if (filters != null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    DataRow dr = dtFilters.NewRow();
                    dr[Constants.FieldNameColumn] = filter.FieldName;
                    dr[Constants.ValueFirstColumn] = filter.ValueFirst;
                    dr[Constants.ValueLastColumn] = filter.ValueLast;
                    dtFilters.Rows.Add(dr);
                }
            }
            var db = this.EImeceDbContext;
            var connection = db.Database.Connection;
            try
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                SqlCommand cmd = (SqlCommand)connection.CreateCommand();
                cmd.CommandText = @"test_SearchProducts";
                cmd.CommandType = CommandType.StoredProcedure;
                var parameterList = new List<SqlParameter>();
                parameterList.Add(DatabaseUtility.GetSqlParameter("search", search.ToStr(), SqlDbType.NVarChar));
                parameterList.Add(DatabaseUtility.GetSqlParameter("filter", dtFilters, SqlDbType.Structured));
                parameterList.Add(DatabaseUtility.GetSqlParameter("top", top, SqlDbType.Int));
                parameterList.Add(DatabaseUtility.GetSqlParameter("skip", skip, SqlDbType.Int));
                parameterList.Add(DatabaseUtility.GetSqlParameter("language", language, SqlDbType.Int));

                cmd.Parameters.AddRange(parameterList.ToArray());
                var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

                var products = ((IObjectContextAdapter)db)
                    .ObjectContext
                    .Translate<Product>(reader, "Products", MergeOption.AppendOnly);

                searchResult.Products = products.OrderByStorefrontDefault().ToList();

                await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);
                var productCategories = ((IObjectContextAdapter)db)
                    .ObjectContext
                    .Translate<ProductCategory>(reader, "ProductCategories", MergeOption.AppendOnly);

                searchResult.ProductCategories = productCategories.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
            finally
            {
                connection.Close();
            }

            searchResult.PageSize = top;
            return searchResult;
        }

        public List<Product> GetChildrenProducts(int[] childrenCategoryId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            Expression<Func<Product, bool>> match = r2 => childrenCategoryId.Contains(r2.ProductCategoryId) && r2.IsActive;
            var result = GetAllIncluding(includeProperties.ToArray())
                .Where(match)
                .OrderByStorefrontDefault()
                .Take(99999)
                .ToList();
            return result.Distinct().ToList();
        }

        public async Task<List<Product>> GetChildrenProductsAsync(int[] childrenCategoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            Expression<Func<Product, bool>> match = r2 => childrenCategoryId.Contains(r2.ProductCategoryId) && r2.IsActive;
            var result = await GetAllIncluding(includeProperties.ToArray())
                .Where(match)
                .OrderByStorefrontDefault()
                .Take(99999)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return result.Distinct().ToList();
        }

        #region Storefront Read Implementations (LINQ Projection, AsNoTracking, Main Entity Activation)

        private static Expression<Func<Product, StorefrontProductCardDto>> ProductCardProjection
        {
            get
            {
                return p => new StorefrontProductCardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    NameShort = p.NameShort,
                    NameLong = p.NameLong,
                    ShortDescription = p.ShortDescription,
                    Price = p.Price,
                    Discount = p.Discount,
                    ProductCode = p.ProductCode,
                    Rating = p.Rating,
                    SoldCount = 0,
                    MainImageId = p.MainImageId,
                    ProductCategoryId = p.ProductCategoryId,
                    ProductCategoryName = p.ProductCategory != null ? p.ProductCategory.Name : string.Empty,
                    BrandId = p.BrandId,
                    BrandName = p.Brand != null ? p.Brand.Name : string.Empty,
                    IsActive = p.IsActive,
                    MainPage = p.MainPage,
                    IsCampaign = p.IsCampaign,
                    State = p.State,
                    Lang = p.Lang,
                    Position = p.Position,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                };
            }
        }

        public async Task<StorefrontProductCardDto> GetStorefrontProductCardByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.Id == id && p.IsActive)
                .Select(ProductCardProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public StorefrontProductCardDto GetStorefrontProductCardById(int id)
        {
            return EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.Id == id && p.IsActive)
                .Select(ProductCardProjection)
                .FirstOrDefault();
        }

        public async Task<StorefrontProductDetailDto> GetStorefrontProductDetailByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dto = await EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new StorefrontProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    NameShort = p.NameShort,
                    NameLong = p.NameLong,
                    ShortDescription = p.ShortDescription,
                    Description = p.Description,
                    Price = p.Price,
                    Discount = p.Discount,
                    ProductCode = p.ProductCode,
                    Rating = p.Rating,
                    SoldCount = 0,
                    MainImageId = p.MainImageId,
                    ProductCategoryId = p.ProductCategoryId,
                    ProductCategoryName = p.ProductCategory != null ? p.ProductCategory.Name : string.Empty,
                    ProductCategoryTemplateId = p.ProductCategory != null ? p.ProductCategory.TemplateId : null,
                    BrandId = p.BrandId,
                    BrandName = p.Brand != null ? p.Brand.Name : string.Empty,
                    IsActive = p.IsActive,
                    MainPage = p.MainPage,
                    IsCampaign = p.IsCampaign,
                    State = p.State,
                    Lang = p.Lang,
                    Position = p.Position,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    VideoUrl = p.VideoUrl,
                    ProductColorOptions = p.ProductColorOptions,
                    ProductSizeOptions = p.ProductSizeOptions,
                    MetaKeywords = p.MetaKeywords,
                    ProductFiles = p.ProductFiles
                        .Where(pf => pf.FileStorage != null && pf.FileStorage.IsActive)
                        .OrderBy(pf => pf.Position)
                        .Select(pf => new StorefrontProductFileDto
                        {
                            Id = pf.Id,
                            ProductId = pf.ProductId,
                            FileStorageId = pf.FileStorageId,
                            FileName = pf.FileStorage.FileName,
                            Title = pf.FileStorage.Name,
                            Description = pf.FileStorage.FileName,
                            Width = pf.FileStorage.Width,
                            Height = pf.FileStorage.Height,
                            Position = pf.Position,
                            IsActive = pf.FileStorage.IsActive
                        }).ToList(),
                    ProductTags = p.ProductTags
                        .Where(pt => pt.Tag != null && pt.Tag.IsActive)
                        .OrderBy(pt => pt.Tag.Position)
                        .Select(pt => new StorefrontTagDto
                        {
                            Id = pt.Tag.Id,
                            Name = pt.Tag.Name,
                            TagCategoryId = pt.Tag.TagCategoryId,
                            TagCategoryName = pt.Tag.TagCategory != null ? pt.Tag.TagCategory.Name : string.Empty,
                            Position = pt.Tag.Position,
                            Lang = pt.Tag.Lang,
                            IsActive = pt.Tag.IsActive
                        }).ToList(),
                    ProductSpecifications = p.ProductSpecifications
                        .OrderBy(ps => ps.Position)
                        .Select(ps => new StorefrontProductSpecificationDto
                        {
                            Id = ps.Id,
                            ProductId = ps.ProductId,
                            Name = ps.Name,
                            Value = ps.Value,
                            Order = ps.Position
                        }).ToList(),
                    ProductComments = p.ProductComments
                        .Where(pc => pc.IsActive)
                        .OrderByDescending(pc => pc.CreatedDate)
                        .Select(pc => new StorefrontProductCommentDto
                        {
                            Id = pc.Id,
                            ProductId = pc.ProductId,
                            Name = pc.Name,
                            Comment = pc.Review,
                            Rating = pc.Rating,
                            CreatedDate = pc.CreatedDate,
                            IsActive = pc.IsActive
                        }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return dto;
        }

        public StorefrontProductDetailDto GetStorefrontProductDetailById(int id)
        {
            var dto = EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new StorefrontProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    NameShort = p.NameShort,
                    NameLong = p.NameLong,
                    ShortDescription = p.ShortDescription,
                    Description = p.Description,
                    Price = p.Price,
                    Discount = p.Discount,
                    ProductCode = p.ProductCode,
                    Rating = p.Rating,
                    SoldCount = 0,
                    MainImageId = p.MainImageId,
                    ProductCategoryId = p.ProductCategoryId,
                    ProductCategoryName = p.ProductCategory != null ? p.ProductCategory.Name : string.Empty,
                    ProductCategoryTemplateId = p.ProductCategory != null ? p.ProductCategory.TemplateId : null,
                    BrandId = p.BrandId,
                    BrandName = p.Brand != null ? p.Brand.Name : string.Empty,
                    IsActive = p.IsActive,
                    MainPage = p.MainPage,
                    IsCampaign = p.IsCampaign,
                    State = p.State,
                    Lang = p.Lang,
                    Position = p.Position,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    VideoUrl = p.VideoUrl,
                    ProductColorOptions = p.ProductColorOptions,
                    ProductSizeOptions = p.ProductSizeOptions,
                    MetaKeywords = p.MetaKeywords,
                    ProductFiles = p.ProductFiles
                        .Where(pf => pf.FileStorage != null && pf.FileStorage.IsActive)
                        .OrderBy(pf => pf.Position)
                        .Select(pf => new StorefrontProductFileDto
                        {
                            Id = pf.Id,
                            ProductId = pf.ProductId,
                            FileStorageId = pf.FileStorageId,
                            FileName = pf.FileStorage.FileName,
                            Title = pf.FileStorage.Name,
                            Description = pf.FileStorage.FileName,
                            Width = pf.FileStorage.Width,
                            Height = pf.FileStorage.Height,
                            Position = pf.Position,
                            IsActive = pf.FileStorage.IsActive
                        }).ToList(),
                    ProductTags = p.ProductTags
                        .Where(pt => pt.Tag != null && pt.Tag.IsActive)
                        .OrderBy(pt => pt.Tag.Position)
                        .Select(pt => new StorefrontTagDto
                        {
                            Id = pt.Tag.Id,
                            Name = pt.Tag.Name,
                            TagCategoryId = pt.Tag.TagCategoryId,
                            TagCategoryName = pt.Tag.TagCategory != null ? pt.Tag.TagCategory.Name : string.Empty,
                            Position = pt.Tag.Position,
                            Lang = pt.Tag.Lang,
                            IsActive = pt.Tag.IsActive
                        }).ToList(),
                    ProductSpecifications = p.ProductSpecifications
                        .OrderBy(ps => ps.Position)
                        .Select(ps => new StorefrontProductSpecificationDto
                        {
                            Id = ps.Id,
                            ProductId = ps.ProductId,
                            Name = ps.Name,
                            Value = ps.Value,
                            Order = ps.Position
                        }).ToList(),
                    ProductComments = p.ProductComments
                        .Where(pc => pc.IsActive)
                        .OrderByDescending(pc => pc.CreatedDate)
                        .Select(pc => new StorefrontProductCommentDto
                        {
                            Id = pc.Id,
                            ProductId = pc.ProductId,
                            Name = pc.Name,
                            Comment = pc.Review,
                            Rating = pc.Rating,
                            CreatedDate = pc.CreatedDate,
                            IsActive = pc.IsActive
                        }).ToList()
                })
                .FirstOrDefault();

            return dto;
        }

        public async Task<List<StorefrontProductCardDto>> GetStorefrontMainPageProductsAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var inStock = ProductState.ProductInStock.ToString();
            var limitedStock = ProductState.LimitedStock.ToString();

            return await EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.MainPage && p.Lang == language && p.MainImageId > 0 &&
                            (p.State == inStock || p.State == limitedStock))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontProductCardDto> GetStorefrontMainPageProducts(int take, int language)
        {
            var inStock = ProductState.ProductInStock.ToString();
            var limitedStock = ProductState.LimitedStock.ToString();

            return EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.MainPage && p.Lang == language && p.MainImageId > 0 &&
                            (p.State == inStock || p.State == limitedStock))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToList();
        }

        public async Task<List<StorefrontProductCardDto>> GetStorefrontLatestProductsAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var inStock = ProductState.ProductInStock.ToString();
            var limitedStock = ProductState.LimitedStock.ToString();

            return await EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Lang == language && p.MainImageId > 0 &&
                            (p.State == inStock || p.State == limitedStock))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontProductCardDto> GetStorefrontLatestProducts(int take, int language)
        {
            var inStock = ProductState.ProductInStock.ToString();
            var limitedStock = ProductState.LimitedStock.ToString();

            return EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Lang == language && p.MainImageId > 0 &&
                            (p.State == inStock || p.State == limitedStock))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToList();
        }

        public async Task<List<StorefrontProductCardDto>> GetStorefrontCampaignProductsAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var inStock = ProductState.ProductInStock.ToString();
            var limitedStock = ProductState.LimitedStock.ToString();

            return await EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.IsCampaign && p.Lang == language && p.MainImageId > 0 &&
                            (p.State == inStock || p.State == limitedStock))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontProductCardDto> GetStorefrontCampaignProducts(int take, int language)
        {
            var inStock = ProductState.ProductInStock.ToString();
            var limitedStock = ProductState.LimitedStock.ToString();

            return EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.IsCampaign && p.Lang == language && p.MainImageId > 0 &&
                            (p.State == inStock || p.State == limitedStock))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToList();
        }

        public async Task<List<StorefrontProductCardDto>> GetStorefrontActiveProductsAsync(int? language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && (!language.HasValue || p.Lang == language.Value) && (p.ProductCategory == null || p.ProductCategory.IsActive))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontProductCardDto> GetStorefrontActiveProducts(int? language)
        {
            return EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && (!language.HasValue || p.Lang == language.Value) && (p.ProductCategory == null || p.ProductCategory.IsActive))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .ToList();
        }

        public async Task<PaginatedList<StorefrontProductCardDto>> GetStorefrontActiveProductsPagedAsync(int pageIndex, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Lang == language)
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault();

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedList<StorefrontProductCardDto>(items, pageIndex, pageSize, totalCount);
        }

        public PaginatedList<StorefrontProductCardDto> GetStorefrontActiveProductsPaged(int pageIndex, int pageSize, int language)
        {
            var inStock = ProductState.ProductInStock.ToString();
            var limitedStock = ProductState.LimitedStock.ToString();

            var query = EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Lang == language && p.MainImageId > 0 &&
                            (p.State == inStock || p.State == limitedStock))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault();

            var totalCount = query.Count();
            var items = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<StorefrontProductCardDto>(items, pageIndex, pageSize, totalCount);
        }

        public async Task<List<StorefrontProductCardDto>> GetStorefrontCategoryProductsAsync(int categoryId, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.ProductCategoryId == categoryId && p.IsActive && p.Lang == language)
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontProductCardDto> GetStorefrontCategoryProducts(int categoryId, int language)
        {
            return EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.ProductCategoryId == categoryId && p.IsActive && p.Lang == language)
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .ToList();
        }

        public async Task<PaginatedList<StorefrontProductCardDto>> GetStorefrontProductsByCategoryIdAsync(
            int categoryId,
            List<int> childCategoryIds,
            int language,
            int pageIndex,
            int pageSize,
            SortingType sorting,
            decimal? minPrice,
            decimal? maxPrice,
            List<int> brandIds,
            List<int> ratings,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var categoryIds = new List<int> { categoryId };
            if (childCategoryIds != null && childCategoryIds.Count > 0)
            {
                categoryIds.AddRange(childCategoryIds);
            }

            var query = EImeceDbContext.Products.AsNoTracking()
                .Where(p => categoryIds.Contains(p.ProductCategoryId) && p.IsActive && p.Lang == language);

            // SQL Price filtering on computed discount price (Price - Discount or Price)
            if (minPrice.HasValue && minPrice.Value > 0)
            {
                var min = minPrice.Value;
                query = query.Where(p => (p.Discount.HasValue && p.Discount.Value > 0 ? (p.Price - p.Discount.Value) : p.Price) >= min);
            }
            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                var max = maxPrice.Value;
                query = query.Where(p => (p.Discount.HasValue && p.Discount.Value > 0 ? (p.Price - p.Discount.Value) : p.Price) <= max);
            }

            // SQL Brand filtering
            if (brandIds != null && brandIds.Count > 0)
            {
                query = query.Where(p => p.BrandId.HasValue && brandIds.Contains(p.BrandId.Value));
            }

            // SQL Rating filtering
            if (ratings != null && ratings.Count > 0)
            {
                query = query.Where(p => ratings.Contains((int)Math.Floor(p.Rating)));
            }

            var projected = query.Select(ProductCardProjection);

            // SQL Sorting
            IOrderedQueryable<StorefrontProductCardDto> ordered;
            switch (sorting)
            {
                case SortingType.LowHighPrice:
                    ordered = projected.OrderBy(t => t.Discount.HasValue && t.Discount.Value > 0 ? (t.Price - t.Discount.Value) : t.Price)
                        .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
                    break;
                case SortingType.HighLowPrice:
                    ordered = projected.OrderByDescending(t => t.Discount.HasValue && t.Discount.Value > 0 ? (t.Price - t.Discount.Value) : t.Price)
                        .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
                    break;
                case SortingType.Newest:
                    ordered = projected.OrderByDescending(t => t.UpdatedDate)
                        .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign);
                    break;
                case SortingType.Popularity:
                    ordered = projected.OrderByDescending(t => t.SoldCount)
                        .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
                    break;
                case SortingType.AverageRating:
                    ordered = projected.OrderByDescending(t => t.Rating)
                        .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
                    break;
                default:
                    ordered = projected.OrderByStorefrontDefault();
                    break;
            }

            var total = await ordered.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedList<StorefrontProductCardDto>(items, pageIndex, pageSize, total);
        }

        public async Task<List<StorefrontProductCardDto>> GetStorefrontRelatedProductsAsync(int[] tagIdList, int take, int language, int excludedProductId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tagIdList == null || tagIdList.Length == 0)
            {
                return new List<StorefrontProductCardDto>();
            }

            return await EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Lang == language && p.Id != excludedProductId &&
                            p.ProductTags.Any(pt => pt.Tag != null && pt.Tag.IsActive && tagIdList.Contains(pt.TagId)))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontProductCardDto> GetStorefrontRelatedProducts(int[] tagIdList, int take, int language, int excludedProductId)
        {
            if (tagIdList == null || tagIdList.Length == 0)
            {
                return new List<StorefrontProductCardDto>();
            }

            return EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Lang == language && p.Id != excludedProductId &&
                            p.ProductTags.Any(pt => pt.Tag != null && pt.Tag.IsActive && tagIdList.Contains(pt.TagId)))
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToList();
        }

        public async Task<List<StorefrontProductCardDto>> GetStorefrontRandomProductsByCategoryIdAsync(int productCategoryId, int take, int language, int excludedProductId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Lang == language && p.ProductCategoryId == productCategoryId && p.Id != excludedProductId)
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontProductCardDto> GetStorefrontRandomProductsByCategoryId(int productCategoryId, int take, int language, int excludedProductId)
        {
            return EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Lang == language && p.ProductCategoryId == productCategoryId && p.Id != excludedProductId)
                .Select(ProductCardProjection)
                .OrderByStorefrontDefault()
                .Take(take)
                .ToList();
        }

        public async Task<PaginatedList<StorefrontProductCardDto>> SearchStorefrontProductsAsync(int pageIndex, int pageSize, string search, int language, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken))
        {
            var term = (search ?? string.Empty).Trim();
            var query = EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Lang == language &&
                            (p.Name.Contains(term) || p.NameLong.Contains(term) || p.NameShort.Contains(term) || p.ProductCode.Contains(term)))
                .Select(ProductCardProjection);

            IOrderedQueryable<StorefrontProductCardDto> ordered;
            if (sorting == SortingType.LowHighPrice)
            {
                ordered = query.OrderBy(t => t.Discount.HasValue && t.Discount.Value > 0 ? (t.Price - t.Discount.Value) : t.Price)
                    .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
            }
            else if (sorting == SortingType.HighLowPrice)
            {
                ordered = query.OrderByDescending(t => t.Discount.HasValue && t.Discount.Value > 0 ? (t.Price - t.Discount.Value) : t.Price)
                    .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
            }
            else if (sorting == SortingType.Newest)
            {
                ordered = query.OrderByDescending(t => t.UpdatedDate).ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign);
            }
            else
            {
                ordered = query.OrderByStorefrontDefault();
            }

            var total = await ordered.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedList<StorefrontProductCardDto>(items, pageIndex, pageSize, total);
        }

        public PaginatedList<StorefrontProductCardDto> SearchStorefrontProducts(int pageIndex, int pageSize, string search, int language, SortingType sorting)
        {
            var term = (search ?? string.Empty).Trim();
            var query = EImeceDbContext.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Lang == language &&
                            (p.Name.Contains(term) || p.NameLong.Contains(term) || p.NameShort.Contains(term) || p.ProductCode.Contains(term)))
                .Select(ProductCardProjection);

            IOrderedQueryable<StorefrontProductCardDto> ordered;
            if (sorting == SortingType.LowHighPrice)
            {
                ordered = query.OrderBy(t => t.Discount.HasValue && t.Discount.Value > 0 ? (t.Price - t.Discount.Value) : t.Price)
                    .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
            }
            else if (sorting == SortingType.HighLowPrice)
            {
                ordered = query.OrderByDescending(t => t.Discount.HasValue && t.Discount.Value > 0 ? (t.Price - t.Discount.Value) : t.Price)
                    .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
            }
            else if (sorting == SortingType.Newest)
            {
                ordered = query.OrderByDescending(t => t.UpdatedDate).ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign);
            }
            else
            {
                ordered = query.OrderByStorefrontDefault();
            }

            var total = ordered.Count();
            var items = ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<StorefrontProductCardDto>(items, pageIndex, pageSize, total);
        }

        public async Task<PaginatedList<StorefrontProductCardDto>> GetStorefrontProductsByTagIdAsync(int tagId, int pageIndex, int pageSize, int language, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Main Entity Activation: Product.IsActive and Tag.IsActive (Tag is checked via TagId / Tag.IsActive)
            var query = EImeceDbContext.ProductTags.AsNoTracking()
                .Where(pt => pt.TagId == tagId && pt.Tag.IsActive && pt.Tag.Lang == language && pt.Product != null && pt.Product.IsActive)
                .Select(pt => new StorefrontProductCardDto
                {
                    Id = pt.Product.Id,
                    Name = pt.Product.Name,
                    NameShort = pt.Product.NameShort,
                    NameLong = pt.Product.NameLong,
                    ShortDescription = pt.Product.ShortDescription,
                    Price = pt.Product.Price,
                    Discount = pt.Product.Discount,
                    ProductCode = pt.Product.ProductCode,
                    Rating = pt.Product.Rating,
                    SoldCount = 0,
                    MainImageId = pt.Product.MainImageId,
                    ProductCategoryId = pt.Product.ProductCategoryId,
                    ProductCategoryName = pt.Product.ProductCategory != null ? pt.Product.ProductCategory.Name : string.Empty,
                    BrandId = pt.Product.BrandId,
                    BrandName = pt.Product.Brand != null ? pt.Product.Brand.Name : string.Empty,
                    IsActive = pt.Product.IsActive,
                    MainPage = pt.Product.MainPage,
                    IsCampaign = pt.Product.IsCampaign,
                    State = pt.Product.State,
                    Lang = pt.Product.Lang,
                    Position = pt.Product.Position,
                    CreatedDate = pt.Product.CreatedDate,
                    UpdatedDate = pt.Product.UpdatedDate
                });

            IOrderedQueryable<StorefrontProductCardDto> ordered;
            if (sorting == SortingType.LowHighPrice)
            {
                ordered = query.OrderBy(t => t.Discount.HasValue && t.Discount.Value > 0 ? (t.Price - t.Discount.Value) : t.Price)
                    .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
            }
            else if (sorting == SortingType.HighLowPrice)
            {
                ordered = query.OrderByDescending(t => t.Discount.HasValue && t.Discount.Value > 0 ? (t.Price - t.Discount.Value) : t.Price)
                    .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
            }
            else if (sorting == SortingType.Newest)
            {
                ordered = query.OrderByDescending(t => t.UpdatedDate).ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign);
            }
            else
            {
                ordered = query.OrderByStorefrontDefault();
            }

            var total = await ordered.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedList<StorefrontProductCardDto>(items, pageIndex, pageSize, total);
        }

        public PaginatedList<StorefrontProductCardDto> GetStorefrontProductsByTagId(int tagId, int pageIndex, int pageSize, int language, SortingType sorting)
        {
            var query = EImeceDbContext.ProductTags.AsNoTracking()
                .Where(pt => pt.TagId == tagId && pt.Tag.IsActive && pt.Tag.Lang == language && pt.Product != null && pt.Product.IsActive)
                .Select(pt => new StorefrontProductCardDto
                {
                    Id = pt.Product.Id,
                    Name = pt.Product.Name,
                    NameShort = pt.Product.NameShort,
                    NameLong = pt.Product.NameLong,
                    ShortDescription = pt.Product.ShortDescription,
                    Price = pt.Product.Price,
                    Discount = pt.Product.Discount,
                    ProductCode = pt.Product.ProductCode,
                    Rating = pt.Product.Rating,
                    SoldCount = 0,
                    MainImageId = pt.Product.MainImageId,
                    ProductCategoryId = pt.Product.ProductCategoryId,
                    ProductCategoryName = pt.Product.ProductCategory != null ? pt.Product.ProductCategory.Name : string.Empty,
                    BrandId = pt.Product.BrandId,
                    BrandName = pt.Product.Brand != null ? pt.Product.Brand.Name : string.Empty,
                    IsActive = pt.Product.IsActive,
                    MainPage = pt.Product.MainPage,
                    IsCampaign = pt.Product.IsCampaign,
                    State = pt.Product.State,
                    Lang = pt.Product.Lang,
                    Position = pt.Product.Position,
                    CreatedDate = pt.Product.CreatedDate,
                    UpdatedDate = pt.Product.UpdatedDate
                });

            IOrderedQueryable<StorefrontProductCardDto> ordered;
            if (sorting == SortingType.LowHighPrice)
            {
                ordered = query.OrderBy(t => t.Discount.HasValue && t.Discount.Value > 0 ? (t.Price - t.Discount.Value) : t.Price)
                    .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
            }
            else if (sorting == SortingType.HighLowPrice)
            {
                ordered = query.OrderByDescending(t => t.Discount.HasValue && t.Discount.Value > 0 ? (t.Price - t.Discount.Value) : t.Price)
                    .ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign).ThenByDescending(t => t.UpdatedDate);
            }
            else if (sorting == SortingType.Newest)
            {
                ordered = query.OrderByDescending(t => t.UpdatedDate).ThenBy(t => t.Position).ThenByDescending(t => t.MainPage).ThenByDescending(t => t.IsCampaign);
            }
            else
            {
                ordered = query.OrderByStorefrontDefault();
            }

            var total = ordered.Count();
            var items = ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<StorefrontProductCardDto>(items, pageIndex, pageSize, total);
        }

        #endregion
    }
}