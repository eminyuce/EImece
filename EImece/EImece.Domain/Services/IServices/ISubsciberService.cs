using EImece.Domain.Entities;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ISubscriberService : IBaseEntityService<Subscriber>
    {
        // Method to get a subscriber by email
        Subscriber GetSubscriberByEmail(string email);

        Task<Subscriber> GetSubscriberByEmailAsync(string email);
    }
}