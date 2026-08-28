using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
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
    public class FaqRepository : BaseEntityRepository<Faq>, IFaqRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public FaqRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking)

        private static Expression<Func<Faq, FaqDto>> FaqProjection
        {
            get
            {
                return f => new FaqDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Question = f.Question,
                    Answer = f.Answer,
                    Position = f.Position,
                    Lang = f.Lang,
                    IsActive = f.IsActive,
                    CreatedDate = f.CreatedDate,
                    UpdatedDate = f.UpdatedDate,
                    AddUserId = f.AddUserId,
                    UpdateUserId = f.UpdateUserId
                };
            }
        }

        [Timed("repo.faq.get_storefront_faqs")]
        public virtual async Task<List<FaqDto>> GetStorefrontFaqsAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Faqs.AsNoTracking()
                .Where(f => f.Lang == language && f.IsActive)
                .OrderBy(f => f.Position)
                .ThenByDescending(f => f.Id)
                .Select(FaqProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        [Timed("repo.faq.get_storefront_faqs_sync")]
        public virtual List<FaqDto> GetStorefrontFaqs(int language)
        {
            return EImeceDbContext.Faqs.AsNoTracking()
                .Where(f => f.Lang == language && f.IsActive)
                .OrderBy(f => f.Position)
                .ThenByDescending(f => f.Id)
                .Select(FaqProjection)
                .ToList();
        }

        private static Expression<Func<Faq, Models.DTOs.Storefront.FaqSummaryDto>> FaqSummaryProjection
        {
            get
            {
                return f => new Models.DTOs.Storefront.FaqSummaryDto
                {
                    Id = f.Id,
                    Question = f.Question,
                    Answer = f.Answer
                };
            }
        }

        [Timed("repo.faq.get_storefront_faq_summaries")]
        public virtual async Task<List<Models.DTOs.Storefront.FaqSummaryDto>> GetStorefrontFaqSummariesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Faqs.AsNoTracking()
                .Where(f => f.Lang == language && f.IsActive)
                .OrderBy(f => f.Position)
                .ThenByDescending(f => f.Id)
                .Select(FaqSummaryProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        [Timed("repo.faq.get_storefront_faq_summaries_sync")]
        public virtual List<Models.DTOs.Storefront.FaqSummaryDto> GetStorefrontFaqSummaries(int language)
        {
            return EImeceDbContext.Faqs.AsNoTracking()
                .Where(f => f.Lang == language && f.IsActive)
                .OrderBy(f => f.Position)
                .ThenByDescending(f => f.Id)
                .Select(FaqSummaryProjection)
                .ToList();
        }

        #endregion
    }
}