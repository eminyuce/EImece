using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class CustomerRepository : BaseEntityRepository<Customer>, ICustomerRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CustomerRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public Customer GetUserId(string userId)
        {
            return EImeceDbContext.Customers.AsNoTracking().FirstOrDefault(r => r.UserId == userId);
        }

        public async Task<Customer> GetUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Customers.AsNoTracking().FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken).ConfigureAwait(false);
        }
    }
}