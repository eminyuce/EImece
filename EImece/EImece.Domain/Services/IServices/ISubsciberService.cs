using EImece.Domain.Entities;

namespace EImece.Domain.Services.IServices
{
    public interface ISubscriberService : IBaseEntityService<Subscriber>
    {
        // Method to get a subscriber by email
        Subscriber GetSubscriberByEmail(string email);
    }
}