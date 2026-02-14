using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Services.IServices;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Services
{
    public class FaqService : BaseEntityService<Faq>, IFaqService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IFaqRepository FaqRepository { get; set; }

        public FaqService(IFaqRepository repository) : base(repository)
        {
            FaqRepository = repository;
        }

        public List<FaqDto> GetActiveBaseEntitiesFromCacheDto(bool fromCache = true, int? lang = null)
        {
            var faqs = GetActiveBaseEntitiesFromCache(fromCache, lang);
            return DtoMappingService.MapListTo<Faq, FaqDto>(faqs);
        }
    }
}