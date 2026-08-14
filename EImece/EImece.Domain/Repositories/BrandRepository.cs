using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class BrandRepository : BaseContentRepository<Brand>, IBrandRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BrandRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        private static Expression<Func<Brand, StorefrontBrandDto>> BrandProjection
        {
            get
            {
                return b => new StorefrontBrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    ShortDescription = b.Description,
                    MainImageId = b.MainImageId,
                    Position = b.Position,
                    Lang = b.Lang,
                    IsActive = b.IsActive,
                    ProductCount = b.Products.Count(p => p.IsActive)
                };
            }
        }

        public async Task<List<StorefrontBrandDto>> GetStorefrontBrandsAsync(int lang, int categoryId = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = EImeceDbContext.Brands.AsNoTracking()
                .Where(r => r.Lang == lang && r.IsActive);

            if (categoryId > 0)
            {
                query = query.Where(r => r.Products.Any(p => p.IsActive && p.ProductCategoryId == categoryId));
            }
            else
            {
                query = query.Where(r => r.Products.Any(p => p.IsActive));
            }

            return await query
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate)
                .Select(BrandProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontBrandDto> GetStorefrontBrands(int lang, int categoryId = 0)
        {
            var query = EImeceDbContext.Brands.AsNoTracking()
                .Where(r => r.Lang == lang && r.IsActive);

            if (categoryId > 0)
            {
                query = query.Where(r => r.Products.Any(p => p.IsActive && p.ProductCategoryId == categoryId));
            }
            else
            {
                query = query.Where(r => r.Products.Any(p => p.IsActive));
            }

            return query
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate)
                .Select(BrandProjection)
                .ToList();
        }

        public async Task<StorefrontBrandDto> GetStorefrontBrandByIdAsync(int brandId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Brands.AsNoTracking()
                .Where(b => b.Id == brandId && b.IsActive)
                .Select(BrandProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public StorefrontBrandDto GetStorefrontBrandById(int brandId)
        {
            return EImeceDbContext.Brands.AsNoTracking()
                .Where(b => b.Id == brandId && b.IsActive)
                .Select(BrandProjection)
                .FirstOrDefault();
        }

        #endregion

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