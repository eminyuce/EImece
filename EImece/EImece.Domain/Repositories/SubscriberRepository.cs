using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Observability.Telemetry;
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

        [Timed("repo.subscribers.get_by_email_sync")]
        public virtual Subscriber GetSubscriberByEmail(string email)
        {
            return this.FindBy(r => r.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        [Timed("repo.subscribers.get_by_email", "Time taken to get subscriber by email from DB")]
        public virtual async Task<Subscriber> GetSubscriberByEmailAsync(string email)
        {
            return await this.FindBy(r => r.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Existence check without materializing the subscriber row.
        /// </summary>
        [Timed("repo.subscribers.exists_by_email", "Time taken to check if subscriber exists by email in DB")]
        public virtual async Task<bool> SubscriberExistsByEmailAsync(string email)
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
        [Timed("repo.subscribers.get_email_by_id", "Time taken to get subscriber email by id from DB")]
        public virtual async Task<string> GetSubscriberEmailByIdAsync(int id)
        {
            return await EImeceDbContext.Subscribers.AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => r.Email)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
    }
}