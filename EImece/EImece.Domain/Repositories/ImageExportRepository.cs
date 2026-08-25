using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    /// <summary>
    /// The only layer allowed to query EF for image-export relation data.
    /// </summary>
    public class ImageExportRepository : IImageExportRepository
    {
        private readonly IEImeceContext _dbContext;

        public ImageExportRepository(IEImeceContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task<List<FileStorage>> GetFileStoragesAsync(CancellationToken cancellationToken)
        {
            return _dbContext.FileStorages
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<List<ProductImageInfo>> GetProductImageInfosAsync(CancellationToken cancellationToken)
        {
            return _dbContext.Products
                .AsNoTracking()
                .Select(p => new ProductImageInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    ProductCode = p.ProductCode,
                    MainImageId = p.MainImageId,
                    ProductCategoryId = p.ProductCategoryId
                })
                .ToListAsync(cancellationToken);
        }

        public Task<List<ProductFileImageInfo>> GetProductFileImageInfosAsync(CancellationToken cancellationToken)
        {
            return _dbContext.ProductFiles
                .AsNoTracking()
                .Select(pf => new ProductFileImageInfo
                {
                    Id = pf.Id,
                    ProductId = pf.ProductId,
                    FileStorageId = pf.FileStorageId,
                    ProductName = pf.Product != null ? pf.Product.Name : null,
                    ProductCode = pf.Product != null ? pf.Product.ProductCode : null
                })
                .ToListAsync(cancellationToken);
        }

        public Task<List<CategoryImageInfo>> GetProductCategoryImageInfosAsync(CancellationToken cancellationToken)
        {
            return _dbContext.ProductCategories
                .AsNoTracking()
                .Select(pc => new CategoryImageInfo
                {
                    Id = pc.Id,
                    Name = pc.Name,
                    MainImageId = pc.MainImageId,
                    ParentId = pc.ParentId
                })
                .ToListAsync(cancellationToken);
        }

        public Task<List<MenuImageInfo>> GetMenuImageInfosAsync(CancellationToken cancellationToken)
        {
            return _dbContext.Menus
                .AsNoTracking()
                .Select(m => new MenuImageInfo
                {
                    Id = m.Id,
                    Name = m.Name,
                    MainImageId = m.MainImageId,
                    MenuLink = m.MenuLink,
                    Link = m.Link
                })
                .ToListAsync(cancellationToken);
        }

        public Task<List<MenuFileImageInfo>> GetMenuFileImageInfosAsync(CancellationToken cancellationToken)
        {
            return _dbContext.MenuFiles
                .AsNoTracking()
                .Select(mf => new MenuFileImageInfo
                {
                    Id = mf.Id,
                    MenuId = mf.MenuId,
                    FileStorageId = mf.FileStorageId,
                    MenuName = mf.Menu != null ? mf.Menu.Name : null
                })
                .ToListAsync(cancellationToken);
        }

        public Task<List<StoryImageInfo>> GetStoryImageInfosAsync(CancellationToken cancellationToken)
        {
            return _dbContext.Stories
                .AsNoTracking()
                .Select(s => new StoryImageInfo
                {
                    Id = s.Id,
                    Name = s.Name,
                    MainImageId = s.MainImageId,
                    StoryCategoryId = s.StoryCategoryId
                })
                .ToListAsync(cancellationToken);
        }

        public Task<List<StoryFileImageInfo>> GetStoryFileImageInfosAsync(CancellationToken cancellationToken)
        {
            return _dbContext.StoryFiles
                .AsNoTracking()
                .Select(sf => new StoryFileImageInfo
                {
                    Id = sf.Id,
                    StoryId = sf.StoryId,
                    FileStorageId = sf.FileStorageId
                })
                .ToListAsync(cancellationToken);
        }

        public Task<List<CategoryImageInfo>> GetStoryCategoryImageInfosAsync(CancellationToken cancellationToken)
        {
            return _dbContext.StoryCategories
                .AsNoTracking()
                .Select(sc => new CategoryImageInfo
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    MainImageId = sc.MainImageId,
                    ParentId = 0
                })
                .ToListAsync(cancellationToken);
        }

        public Task<List<BrandImageInfo>> GetBrandImageInfosAsync(CancellationToken cancellationToken)
        {
            return _dbContext.Brands
                .AsNoTracking()
                .Select(b => new BrandImageInfo
                {
                    Id = b.Id,
                    Name = b.Name,
                    MainImageId = b.MainImageId
                })
                .ToListAsync(cancellationToken);
        }
    }
}
