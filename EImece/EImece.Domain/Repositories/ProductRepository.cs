using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
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
    }
}