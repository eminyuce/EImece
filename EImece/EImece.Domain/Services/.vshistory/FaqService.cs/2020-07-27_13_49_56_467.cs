using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;

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
    }
}