using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository;
using GenericRepository.EntityFramework.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

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
                Expression<Func<Product, int>> keySelector = t => t.Position;
                var items = this.PaginateDescending(pageIndex, pageSize, keySelector, match, includeProperties);

                return items;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, exception.Message);
                throw;
            }
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
                Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.MainPage && r2.Lang == language;
                Expression<Func<Product, int>> keySelector = t => t.Position;
                var items = this.PaginateDescending(pageIndex, pageSize, keySelector, match, includeProperties);

                return items;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, exception.Message);
                throw;
            }
        }

        public List<Product> GetAdminPageList(int categoryId, string search, int language)
        {
            Expression<Func<Product, object>> includeProperty4 = r => r.ProductComments;
            Expression<Func<Product, object>> includeProperty3 = r => r.MainImage;
            Expression<Func<Product, object>> includeProperty2 = r => r.ProductCategory;
            Expression<Func<Product, object>>[] includeProperties = { includeProperty2, includeProperty3, includeProperty4 };
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
            if (categoryId > 0)
            {
                products = products.Where(r => r.ProductCategoryId == categoryId);
            }
            else
            {
                // CategoryId is -1 for excel exporting.
                if (String.IsNullOrEmpty(search) && categoryId != -1)
                {
                    products = products.Take(1000);
                }
            }
            products = products.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);

            return products.ToList();
        }

        public Product GetProduct(int id)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductComments);
            includeProperties.Add(r => r.ProductFiles.Select(q => q.FileStorage));
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.ProductTags.Select(q => q.Tag));
            includeProperties.Add(r => r.ProductSpecifications);
            var item = GetSingleIncluding(id, includeProperties.ToArray());

            return item;
        }

        public PaginatedList<Product> SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting)
        {
            String searchKey = search.ToStr().Trim();
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.ProductTags.Select(q => q.Tag));
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == lang 
            && (r2.Name.Contains(search.Trim()) || r2.NameLong.Contains(search.Trim()));

            if (sorting == SortingType.LowHighPrice)
            {
                Expression<Func<Product, decimal>> keySelector = t => t.Price;
                return this.Paginate(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());
            }
            else if (sorting == SortingType.HighLowPrice)
            {
                Expression<Func<Product, decimal>> keySelector = t => t.Price;
                return this.PaginateDescending(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());
            }
            else if (sorting == SortingType.Newest)
            {
                Expression<Func<Product, DateTime>> keySelector = t => t.UpdatedDate;
                return this.Paginate(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());
            }
            else
            {
                Expression<Func<Product, double>> keySelector = t => t.Position;
                return this.Paginate(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());
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
            Expression<Func<Product, int>> keySelector = t => t.Position;
            var result = FindAllIncluding(match, keySelector, OrderByType.Descending, take, 0, includeProperties.ToArray());
            var result2 = result.ToList();
            return result2.Distinct().ToList();
        }

        public List<Product> GetRandomProductsByCategoryId(int productCategoryId, int take, int lang, int excludedProductId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == lang && r2.ProductCategoryId == productCategoryId && r2.Id != excludedProductId;
            Expression<Func<Product, int>> keySelector = t => t.Position;
            var result = FindAllIncluding(match, keySelector, OrderByType.Descending, take, 0, includeProperties.ToArray());
            var result2 = result.ToList();
            return result2.Distinct().ToList();
        }

        public List<Product> GetActiveProducts(int? language)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == language && r2.ProductCategory.IsActive;
            Expression<Func<Product, int>> keySelector = t => t.Position;
            var result = FindAllIncluding(match, keySelector, OrderByType.Descending, null, null, includeProperties.ToArray());

            return result.ToList();
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

            var result = GetProductsSearchResult(search, fltrs, top, skip, language);

            return result;
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

            dtFilters.Columns.Add("FieldName");
            dtFilters.Columns.Add("ValueFirst");
            dtFilters.Columns.Add("ValueLast");

            if (filters != null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    DataRow dr = dtFilters.NewRow();
                    dr["FieldName"] = filter.FieldName;
                    dr["ValueFirst"] = filter.ValueFirst;
                    dr["ValueLast"] = filter.ValueLast;
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

                searchResult.Products = products.ToList();

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

        public List<Product> GetChildrenProducts(int[] childrenCategoryId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            Expression<Func<Product, bool>> match = r2 => childrenCategoryId.Contains(r2.ProductCategoryId) && r2.IsActive;
            Expression<Func<Product, int>> keySelector = t => t.Position;
            var result = FindAllIncluding(match, keySelector, OrderByType.Ascending, 99999, 0, includeProperties.ToArray());
            var result2 = result.ToList();
            return result2.Distinct().ToList();
        }
    }
}