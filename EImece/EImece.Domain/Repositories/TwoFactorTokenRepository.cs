using EImece.Domain.DbContext;
using EImece.Domain.Entities;
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

        public async Task RemoveUnusedByUserIdAsync(string userId)
        {
            var oldTokens = _db.TwoFactorTokens
                .Where(t => t.UserId == userId && !t.IsUsed);
            _db.TwoFactorTokens.RemoveRange(oldTokens);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task AddAsync(TwoFactorToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            _db.TwoFactorTokens.Add(token);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<TwoFactorToken> FindUnusedByTokenAsync(string token)
        {
            return _db.TwoFactorTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);
        }

        public async Task DeleteAsync(TwoFactorToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            _db.TwoFactorTokens.Remove(token);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteExpiredAndUsedAsync()
        {
            var expired = _db.TwoFactorTokens
                .Where(t => t.ExpiresUtc < DateTime.UtcNow || t.IsUsed);
            _db.TwoFactorTokens.RemoveRange(expired);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<int> SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
