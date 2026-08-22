using EImece.Domain.Entities;
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

        public Subscriber GetSubscriberByEmail(string email)
        {
            return SubscriberRepository.GetSubscriberByEmail(email);
        }

        public async Task<Subscriber> GetSubscriberByEmailAsync(string email)
        {
            return await SubscriberRepository.GetSubscriberByEmailAsync(email).ConfigureAwait(false);
        }

        public async Task<bool> SubscriberExistsByEmailAsync(string email)
        {
            return await SubscriberRepository.SubscriberExistsByEmailAsync(email).ConfigureAwait(false);
        }

        public async Task<string> GetSubscriberEmailByIdAsync(int id)
        {
            return await SubscriberRepository.GetSubscriberEmailByIdAsync(id).ConfigureAwait(false);
        }
    }
}