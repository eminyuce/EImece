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

        /// <summary>
        /// Existence check without materializing the subscriber row.
        /// </summary>
        public async Task<bool> SubscriberExistsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            return await this.GetAllReadOnly()
                .AnyAsync(r => r.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Single-column projection used by the thank-you page.
        /// </summary>
        public async Task<string> GetSubscriberEmailByIdAsync(int id)
        {
            return await EImeceDbContext.Subscribers.AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => r.Email)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
    }
}