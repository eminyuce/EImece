using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Linq;

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
            var item = FindBy(r => r.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            return item;
        }
    }
}