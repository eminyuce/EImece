using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Observability.Telemetry;
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
                    Description = b.Description,
                    Url = b.Link,
                    MainImageId = b.MainImageId
                };
            }
        }

        [Timed("repo.main_page_image.get_banners", "Time taken to get storefront main page banners from DB")]
        public virtual async Task<List<StorefrontBannerDto>> GetStorefrontMainPageBannersAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.MainPageImages.AsNoTracking()
                .Where(r => r.Lang == language && r.IsActive)
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate)
                .Select(BannerProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        [Timed("repo.main_page_image.get_banners_sync")]
        public virtual List<StorefrontBannerDto> GetStorefrontMainPageBanners(int language)
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