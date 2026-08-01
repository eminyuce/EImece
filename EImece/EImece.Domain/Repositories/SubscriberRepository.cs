using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class SubscriberRepository : BaseEntityRepository<Subscriber>, ISubscriberRepository
    {
        public SubscriberRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public Subscriber GetSubscriberByEmail(string email)
        {
            return this.FindBy(r => r.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public async Task<Subscriber> GetSubscriberByEmailAsync(string email)
        {
            return await this.FindBy(r => r.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefaultAsync().ConfigureAwait(false);
        }
    }
}