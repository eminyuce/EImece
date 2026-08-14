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
    public class MainPageImageRepository : BaseContentRepository<MainPageImage>, IMainPageImageRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MainPageImageRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        private static Expression<Func<MainPageImage, StorefrontBannerDto>> BannerProjection
        {
            get
            {
                return b => new StorefrontBannerDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Title = b.Name,
                    Description = b.Description,
                    ShortDescription = b.Description,
                    Url = b.Link,
                    MainImageId = b.MainImageId,
                    Position = b.Position,
                    Lang = b.Lang,
                    IsActive = b.IsActive
                };
            }
        }

        public async Task<List<StorefrontBannerDto>> GetStorefrontMainPageBannersAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.MainPageImages.AsNoTracking()
                .Where(r => r.Lang == language && r.IsActive)
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate)
                .Select(BannerProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontBannerDto> GetStorefrontMainPageBanners(int language)
        {
            return EImeceDbContext.MainPageImages.AsNoTracking()
                .Where(r => r.Lang == language && r.IsActive)
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate)
                .Select(BannerProjection)
                .ToList();
        }

        #endregion
    }
}