using EImece.Domain.Entities;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ISubscriberRepository : IBaseEntityRepository<Subscriber>
    {
        Subscriber GetSubscriberByEmail(string email);

        Task<Subscriber> GetSubscriberByEmailAsync(string email);
    }
}