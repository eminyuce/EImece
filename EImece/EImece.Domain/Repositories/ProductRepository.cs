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
using NLog;
using Ninject;

namespace EImece.Domain.Repositories
{
    public class ProductRepository : BaseRepository<Product, int>, IProductRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IProductTagRepository ProductTagRepository { get; set; }

        public ProductRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public PaginatedList<Product> GetMainPageProducts(int page, int language)
        {
            try
            {

                int pageIndex = page;
                int pageSize = Settings.RecordPerPage;

                Expression<Func<Product, object>> includeProperty1 = r => r.ProductFiles;
                Expression<Func<Product, object>> includeProperty2 = r => r.ProductCategory;
                Expression<Func<Product, object>> includeProperty3 = r => r.MainImage;
                Expression<Func<Product, object>>[] includeProperties = { includeProperty1, includeProperty2, includeProperty3 };
                Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.MainPage  && r2.Lang == language;
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
            ProductTagRepository.DeleteProductTags(item.Id);
            return BaseEntityRepository.DeleteItem(this, item);
        }

        public List<Product> GetAdminPageList(int categoryId, string search, int language)
        {
            var products = GetAll().Where(r => r.Lang == language);
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

        public List<Product> GetActiveBaseEntities(bool? isActive)
        {
            return BaseEntityRepository.GetActiveBaseEntities(this, isActive);
        }

        public List<Product> GetActiveBaseContents(bool? isActive, int language)
        {
            return BaseContentRepository.GetActiveBaseContents(this, isActive, language);
        }
    }
}
