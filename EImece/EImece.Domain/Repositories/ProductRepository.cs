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
using EImece.Domain.GenericRepositories;
using GenericRepository;

namespace EImece.Domain.Repositories
{
    public class ProductRepository : BaseRepository<Product, int>, IProductRepository
    {
        public ProductRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public PaginatedList<Product> GetMainPageProducts(int page)
        {
            try
            {

                int pageIndex = page;
                int pageSize = Settings.RecordPerPage;

                Expression<Func<Product, object>> includeProperty1 = r => r.ProductFiles;
                Expression<Func<Product, object>> includeProperty2 = r => r.ProductCategory;
                Expression<Func<Product, object>>[] includeProperties = { includeProperty1, includeProperty2 };
                Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.MainPage;
                Expression<Func<Product, int>> keySelector = t => t.Position;
                var items = this.PaginateDescending(pageIndex, pageSize, keySelector , match, includeProperties);

                return items;
            }
            catch (Exception exception)
            {
                Logger.Error(exception,exception.Message);
                throw exception;
            }

        }

       

        public int SaveOrEdit(Product item)
        {
            return BaseEntityRepository.SaveOrEdit(this, item);
        }

        public int DeleteItem(Product item)
        {
            return BaseEntityRepository.DeleteItem(this, item);
        }

        public List<Product> GetAdminPageList(int categoryId, string search)
        {
            var products = GetAll();
            if (!String.IsNullOrEmpty(search))
            {
                products = products.Where(r => r.Name.ToLower().Contains(search) || r.ProductCode.ToLower().Contains(search));
            }
            if (categoryId > 0)
            {
                products = products.Where(r => r.ProductCategoryId == categoryId);
            }
            products = products.OrderBy(r => r.Position);

            return products.ToList();
        }
    }
}
