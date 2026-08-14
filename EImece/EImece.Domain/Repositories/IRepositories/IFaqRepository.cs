using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IFaqRepository : IBaseEntityRepository<Faq>
    {
        #region Storefront Read Methods (LINQ Projection, AsNoTracking)

        Task<List<FaqDto>> GetStorefrontFaqsAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
        List<FaqDto> GetStorefrontFaqs(int language);

        #endregion
    }
}