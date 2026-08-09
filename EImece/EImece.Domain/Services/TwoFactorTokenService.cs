using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    /// <summary>
    /// Short-lived, single-use DB tokens for the TOTP verification step after password check.
    /// </summary>
    public class TwoFactorTokenService
    {
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(8);

        private readonly ApplicationDbContext _db;

        public TwoFactorTokenService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> CreateTokenAsync(string userId)
        {
            var oldTokens = _db.TwoFactorTokens
                .Where(t => t.UserId == userId && !t.IsUsed);
            _db.TwoFactorTokens.RemoveRange(oldTokens);

            var tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }

            string token = System.Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            var entity = new TwoFactorToken
            {
                UserId = userId,
                Token = token,
                ExpiresUtc = DateTime.UtcNow.Add(TokenLifetime),
                IsUsed = false
            };

            _db.TwoFactorTokens.Add(entity);
            await _db.SaveChangesAsync();

            return token;
        }

        /// <summary>
        /// Validates and consumes the token. Returns userId if valid; otherwise null.
        /// </summary>
        public async Task<string> ValidateAndConsumeTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var entity = await _db.TwoFactorTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);

            if (entity == null)
            {
                return null;
            }

            if (entity.ExpiresUtc < DateTime.UtcNow)
            {
                _db.TwoFactorTokens.Remove(entity);
                await _db.SaveChangesAsync();
                return null;
            }

            entity.IsUsed = true;
            await _db.SaveChangesAsync();

            return entity.UserId;
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expired = _db.TwoFactorTokens
                .Where(t => t.ExpiresUtc < DateTime.UtcNow || t.IsUsed);
            _db.TwoFactorTokens.RemoveRange(expired);
            await _db.SaveChangesAsync();
        }
    }
}
