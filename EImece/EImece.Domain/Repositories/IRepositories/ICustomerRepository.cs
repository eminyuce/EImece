using EImece.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ICustomerRepository : IBaseEntityRepository<Customer>
    {
        Customer GetUserId(string userId);

        Task<Customer> GetUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken));
    }
}