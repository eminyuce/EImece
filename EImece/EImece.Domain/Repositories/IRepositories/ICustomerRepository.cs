using EImece.Domain.Entities;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ICustomerRepository : IBaseEntityRepository<Customer>
    {
        Customer GetUserId(string userId);
    }
}