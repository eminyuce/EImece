using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using System.Linq.Expressions;
using GenericRepository.EntityFramework.Enums;
using GenericRepository;
using NLog;
using Ninject;
using EImece.Domain.Helpers;

namespace EImece.Domain.Repositories
{
    public class ProductRepository : BaseContentRepository<Product>, IProductRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public ProductRepository(IEImeceContext dbContext) : base(dbContext)
        {
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
            Expression<Func<Product, object>> includeProperty3 = r => r.MainImage;
            Expression<Func<Product, object>> includeProperty2 = r => r.ProductCategory;
            Expression<Func<Product, object>>[] includeProperties = { includeProperty2, includeProperty3 };
            var products = GetAllIncluding(includeProperties).Where(r => r.Lang == language);
            search = search.ToStr().ToLower().Trim();
            if (!String.IsNullOrEmpty(search))
            {

                products = products.Where(r => r.Name.ToLower().Contains(search) || r.ProductCode.ToLower().Contains(search) || r.ProductCategory.Name.ToLower().Contains(search));
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
            includeProperties.Add(r => r.ProductFiles.Select(q => q.FileStorage));
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.ProductTags.Select(q => q.Tag));
            includeProperties.Add(r => r.ProductSpecifications);
            var item = GetSingleIncluding(id, includeProperties.ToArray());

            return item;
        }

        public PaginatedList<Product> SearchProducts(int pageIndex, int pageSize, string search, int lang)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.ProductTags.Select(q => q.Tag));
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == lang && r2.Name.Contains(search.Trim());
            Expression<Func<Product, int>> keySelector = t => t.Position;
            var items = this.Paginate(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());

            return items;
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
            var result = FindAllIncluding(match, keySelector, OrderByType.Ascending, take, 0, includeProperties.ToArray());

            return result.ToList();
        }

        public IQueryable<Product> GetActiveProducts(bool? isActive, int? language)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == language;
            Expression<Func<Product, int>> keySelector = t => t.Position;
            var result = FindAllIncluding(match, keySelector, OrderByType.Ascending, null,null, includeProperties.ToArray());

            return result;
        }
    }
}
