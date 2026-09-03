using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class TwoFactorTokenServiceTests
    {
        private class TokenStore
        {
            public List<TwoFactorToken> Tokens { get; } = new List<TwoFactorToken>();
            public int SaveChangesCalls { get; private set; }
            public int CleanupCalls { get; private set; }

            public Task RemoveUnusedByUserIdAsync(string userId)
            {
                Tokens.RemoveAll(t => t.UserId == userId && !t.IsUsed);
                return Task.CompletedTask;
            }

            public Task AddAsync(TwoFactorToken token)
            {
                token.Id = Tokens.Count + 1;
                Tokens.Add(token);
                return Task.CompletedTask;
            }

            public Task<TwoFactorToken> FindUnusedByTokenAsync(string token)
            {
                return Task.FromResult(Tokens.FirstOrDefault(t => t.Token == token && !t.IsUsed));
            }

            public Task DeleteAsync(TwoFactorToken token)
            {
                Tokens.Remove(token);
                return Task.CompletedTask;
            }

            public Task DeleteExpiredAndUsedAsync()
            {
                CleanupCalls++;
                Tokens.RemoveAll(t => t.IsUsed || t.ExpiresUtc < DateTime.UtcNow);
                return Task.CompletedTask;
            }

            public Task<int> SaveChangesAsync()
            {
                SaveChangesCalls++;
                return Task.FromResult(1);
            }
        }

        private static TwoFactorTokenService CreateService(TokenStore store)
        {
            return new TwoFactorTokenService(new FakeServiceProxy<ITwoFactorTokenRepository>(store).Instance);
        }

        [TestMethod]
        public void Constructor_ThrowsWhenRepositoryIsNull()
        {
            try
            {
                var unused = new TwoFactorTokenService(null);
                Assert.Fail("Expected ArgumentNullException.");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [TestMethod]
        public async Task CreateTokenAsync_ReplacesUnusedTokensAndReturnsUrlSafeValue()
        {
            var store = new TokenStore();
            store.Tokens.Add(new TwoFactorToken { UserId = "u1", Token = "old", IsUsed = false, ExpiresUtc = DateTime.UtcNow.AddMinutes(5) });
            store.Tokens.Add(new TwoFactorToken { UserId = "u1", Token = "used", IsUsed = true, ExpiresUtc = DateTime.UtcNow.AddMinutes(5) });
            var service = CreateService(store);

            var token = await service.CreateTokenAsync("u1");

            Assert.IsFalse(string.IsNullOrWhiteSpace(token));
            Assert.IsFalse(token.Contains("+"));
            Assert.IsFalse(token.Contains("/"));
            Assert.IsFalse(token.EndsWith("="));
            Assert.IsFalse(store.Tokens.Any(t => t.Token == "old"));
            Assert.IsTrue(store.Tokens.Any(t => t.Token == "used"));
            var created = store.Tokens.Single(t => t.Token == token);
            Assert.AreEqual("u1", created.UserId);
            Assert.IsFalse(created.IsUsed);
            Assert.IsTrue(created.ExpiresUtc > DateTime.UtcNow.AddMinutes(7));
            Assert.IsTrue(created.ExpiresUtc <= DateTime.UtcNow.AddMinutes(8).AddSeconds(5));
        }

        [TestMethod]
        public async Task ValidateAndConsumeTokenAsync_ReturnsNullForBlankOrUnknownToken()
        {
            var service = CreateService(new TokenStore());

            Assert.IsNull(await service.ValidateAndConsumeTokenAsync(null));
            Assert.IsNull(await service.ValidateAndConsumeTokenAsync(" "));
            Assert.IsNull(await service.ValidateAndConsumeTokenAsync("missing"));
        }

        [TestMethod]
        public async Task ValidateAndConsumeTokenAsync_DeletesExpiredTokenWithoutReturningUser()
        {
            var store = new TokenStore();
            store.Tokens.Add(new TwoFactorToken
            {
                UserId = "u1",
                Token = "expired",
                IsUsed = false,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(-1)
            });
            var service = CreateService(store);

            var userId = await service.ValidateAndConsumeTokenAsync("expired");

            Assert.IsNull(userId);
            Assert.IsFalse(store.Tokens.Any(t => t.Token == "expired"));
            Assert.AreEqual(0, store.SaveChangesCalls);
        }

        [TestMethod]
        public async Task ValidateAndConsumeTokenAsync_MarksValidTokenUsedOnce()
        {
            var store = new TokenStore();
            store.Tokens.Add(new TwoFactorToken
            {
                UserId = "u9",
                Token = "live",
                IsUsed = false,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(4)
            });
            var service = CreateService(store);

            var first = await service.ValidateAndConsumeTokenAsync("live");
            var second = await service.ValidateAndConsumeTokenAsync("live");

            Assert.AreEqual("u9", first);
            Assert.IsTrue(store.Tokens.Single(t => t.Token == "live").IsUsed);
            Assert.AreEqual(1, store.SaveChangesCalls);
            Assert.IsNull(second);
        }

        [TestMethod]
        public async Task CleanupExpiredTokensAsync_DelegatesToRepository()
        {
            var store = new TokenStore();
            store.Tokens.Add(new TwoFactorToken { Token = "stale", IsUsed = true, ExpiresUtc = DateTime.UtcNow.AddMinutes(-10) });
            var service = CreateService(store);

            await service.CleanupExpiredTokensAsync();

            Assert.AreEqual(1, store.CleanupCalls);
            Assert.AreEqual(0, store.Tokens.Count);
        }
    }
}
