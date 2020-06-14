using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;

namespace EImece.Domain.Services
{
    public class SubscriberService : BaseEntityService<Subscriber>, ISubscriberService
    {
        private ISubscriberRepository SubscriberRepository { get; set; }

        public SubscriberService(ISubscriberRepository
            repository) : base(repository)
        {
            SubscriberRepository = repository;
        }
    }
}