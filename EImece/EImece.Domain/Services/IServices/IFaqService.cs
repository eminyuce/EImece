using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface IFaqService : IBaseEntityService<Faq>
    {
        List<FaqDto> GetActiveBaseEntitiesFromCacheDto(bool fromCache = true, int? lang = null);
    }
}