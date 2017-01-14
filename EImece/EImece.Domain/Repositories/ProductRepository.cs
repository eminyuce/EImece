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
                Expression<Func<Product, object>>[] includeProperties = { includeProperty1, includeProperty2, includeProperty3 };
                Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.MainPage && r2.Lang == language;
                Expression<Func<Product, int>> keySelector = t => t.Position;
                var items = this.PaginateDescending(pageIndex, pageSize, keySelector, match, includeProperties);

                return items;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, exception.Message);
                throw exception;
            }

        }


        public List<Product> GetAdminPageList(int categoryId, string search, int language)
        {
            Expression<Func<Product, object>> includeProperty3 = r => r.MainImage;
            Expression<Func<Product, object>> includeProperty2 = r => r.ProductCategory;
            Expression<Func<Product, object>>[] includeProperties = { includeProperty2, includeProperty3 };
            var products = GetAllIncluding(includeProperties).Where(r => r.Lang == language);
            if (!String.IsNullOrEmpty(search))
            {
                products = products.Where(r => r.Name.ToLower().Contains(search) || r.ProductCode.ToLower().Contains(search));
            }
            if (categoryId > 0)
            {
                products = products.Where(r => r.ProductCategoryId == categoryId);
            }
            products = products.OrderBy(r => r.Position).ThenByDescending(r => r.Id);

            return products.ToList();
        }

        public Product GetProduct(int id)
        {
            var includeProperties = GetIncludePropertyExpressionList<Product>();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductFiles.Select(q => q.FileStorage));
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.ProductTags.Select(q => q.Tag));
            includeProperties.Add(r => r.ProductSpecifications);
            var item = GetSingleIncluding(id, includeProperties.ToArray());

            return item;
        }

        public PaginatedList<Product> SearchProducts(int pageIndex,int pageSize,string search, int lang)
        {
            var includeProperties = GetIncludePropertyExpressionList<Product>();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.ProductCategory);
            includeProperties.Add(r => r.ProductTags.Select(q => q.Tag));
            Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == lang && r2.Name.Trim().ToLower().Contains(search.Trim().ToLower());
            Expression<Func<Product, int>> keySelector = t => t.Position;
            var items = this.PaginateDescending(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());

            return items;
        }
    }
}
