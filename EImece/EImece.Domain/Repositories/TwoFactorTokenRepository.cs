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
    /// <summary>
    /// Data access for short-lived two-factor verification tokens.
    /// Wraps ApplicationDbContext; the only layer allowed to touch EF Core/EF6 context types.
    /// </summary>
    public class TwoFactorTokenRepository : ITwoFactorTokenRepository
    {
        private readonly ApplicationDbContext _db;

        public TwoFactorTokenRepository(ApplicationDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        [Timed("repo.two_factor_token.remove_unused_by_user")]
        public virtual async Task RemoveUnusedByUserIdAsync(string userId)
        {
            var oldTokens = _db.TwoFactorTokens
                .Where(t => t.UserId == userId && !t.IsUsed);
            _db.TwoFactorTokens.RemoveRange(oldTokens);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        [Timed("repo.two_factor_token.add")]
        public virtual async Task AddAsync(TwoFactorToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            _db.TwoFactorTokens.Add(token);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        [Timed("repo.two_factor_token.find_unused_by_token")]
        public virtual Task<TwoFactorToken> FindUnusedByTokenAsync(string token)
        {
            return _db.TwoFactorTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);
        }

        [Timed("repo.two_factor_token.delete")]
        public virtual async Task DeleteAsync(TwoFactorToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            _db.TwoFactorTokens.Remove(token);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        [Timed("repo.two_factor_token.delete_expired_and_used")]
        public virtual async Task DeleteExpiredAndUsedAsync()
        {
            var expired = _db.TwoFactorTokens
                .Where(t => t.ExpiresUtc < DateTime.UtcNow || t.IsUsed);
            _db.TwoFactorTokens.RemoveRange(expired);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        [Timed("repo.two_factor_token.save_changes")]
        public virtual Task<int> SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
