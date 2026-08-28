using EImece.Domain.Entities;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using System;
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

        private readonly ITwoFactorTokenRepository _tokenRepository;

        public TwoFactorTokenService(ITwoFactorTokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
        }

        [Timed("service.two_factor_token.create")]
        public virtual async Task<string> CreateTokenAsync(string userId)
        {
            await _tokenRepository.RemoveUnusedByUserIdAsync(userId).ConfigureAwait(false);

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

            await _tokenRepository.AddAsync(entity).ConfigureAwait(false);

            return token;
        }

        /// <summary>
        /// Validates and consumes the token. Returns userId if valid; otherwise null.
        /// </summary>
        [Timed("service.two_factor_token.validate_and_consume")]
        public virtual async Task<string> ValidateAndConsumeTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var entity = await _tokenRepository.FindUnusedByTokenAsync(token).ConfigureAwait(false);

            if (entity == null)
            {
                return null;
            }

            if (entity.ExpiresUtc < DateTime.UtcNow)
            {
                await _tokenRepository.DeleteAsync(entity).ConfigureAwait(false);
                return null;
            }

            entity.IsUsed = true;
            await _tokenRepository.SaveChangesAsync().ConfigureAwait(false);

            return entity.UserId;
        }

        [Timed("service.two_factor_token.cleanup_expired")]
        public virtual async Task CleanupExpiredTokensAsync()
        {
            await _tokenRepository.DeleteExpiredAndUsedAsync().ConfigureAwait(false);
        }
    }
}
