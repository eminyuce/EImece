using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class BrandRepository : BaseContentRepository<Brand>, IBrandRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BrandRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<Brand> GetAdminPageList(string search, int lang)
        {
            Expression<Func<Brand, object>> includeProperty3 = r => r.MainImage;
            Expression<Func<Brand, object>>[] includeProperties = { includeProperty3 };
            var brands = GetAllIncluding(includeProperties).Where(r => r.Lang == lang);
            if (!String.IsNullOrEmpty(search))
            {
                brands = brands.Where(r => r.Name.Contains(search));
            }
            brands = brands.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);

            return brands.ToList();
        }

        public async Task<List<Brand>> GetAdminPageListAsync(string search, int lang)
        {
            Expression<Func<Brand, object>> includeProperty3 = r => r.MainImage;
            Expression<Func<Brand, object>>[] includeProperties = { includeProperty3 };
            var brands = GetAllIncluding(includeProperties).Where(r => r.Lang == lang);
            if (!String.IsNullOrEmpty(search))
            {
                brands = brands.Where(r => r.Name.Contains(search));
            }
            brands = brands.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);

            return await brands.ToListAsync().ConfigureAwait(false);
        }

        public List<Brand> GetBrandsIfAnyProductExists(int lang, int categoryId = 0)
        {
            var brandsWithProducts = GetAllReadOnly()
                .Where(r => r.Lang == lang &&
                    (categoryId > 0
                        ? r.Products.Any(p => p.ProductCategoryId == categoryId)
                        : r.Products.Any()))
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate);

            return brandsWithProducts.ToList();
        }

        public async Task<List<Brand>> GetBrandsIfAnyProductExistsAsync(int lang, int categoryId = 0)
        {
            var brandsWithProducts = GetAllReadOnly()
                .Where(r => r.Lang == lang &&
                    (categoryId > 0
                        ? r.Products.Any(p => p.ProductCategoryId == categoryId)
                        : r.Products.Any()))
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate);

            return await brandsWithProducts.ToListAsync().ConfigureAwait(false);
        }
    }
}