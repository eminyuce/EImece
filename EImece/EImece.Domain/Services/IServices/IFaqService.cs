using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IFaqService : IBaseEntityService<Faq>
    {
        #region Storefront Read Methods (LINQ Projection, AsNoTracking)

        Task<List<FaqDto>> GetStorefrontFaqsAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
        List<FaqDto> GetStorefrontFaqs(int language);

        #endregion
    }
}