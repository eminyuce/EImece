using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ICustomerRepository : IBaseEntityRepository<Customer>
    {
        Customer GetUserId(string userId);

        Task<Customer> GetUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken));

        Task<CustomerSummaryDto> GetStorefrontCustomerSummaryByUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken));

        Task<CustomerDto> GetStorefrontCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> PromoteCustomerToNormalTypeAsync(string userId, int normalCustomerType);
    }
}