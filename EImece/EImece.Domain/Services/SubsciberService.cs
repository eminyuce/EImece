using EImece.Domain.Entities;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using System.Threading.Tasks;

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

        [Timed("service.subscribers.get_by_email_sync")]
        public virtual Subscriber GetSubscriberByEmail(string email)
        {
            return SubscriberRepository.GetSubscriberByEmail(email);
        }

        [Timed("service.subscribers.get_by_email", "Time taken to get subscriber by email")]
        public virtual async Task<Subscriber> GetSubscriberByEmailAsync(string email)
        {
            return await SubscriberRepository.GetSubscriberByEmailAsync(email).ConfigureAwait(false);
        }

        [Timed("service.subscribers.exists_by_email", "Time taken to check if subscriber exists by email")]
        public virtual async Task<bool> SubscriberExistsByEmailAsync(string email)
        {
            return await SubscriberRepository.SubscriberExistsByEmailAsync(email).ConfigureAwait(false);
        }

        [Timed("service.subscribers.get_email_by_id", "Time taken to get subscriber email by id")]
        public virtual async Task<string> GetSubscriberEmailByIdAsync(int id)
        {
            return await SubscriberRepository.GetSubscriberEmailByIdAsync(id).ConfigureAwait(false);
        }
    }
}