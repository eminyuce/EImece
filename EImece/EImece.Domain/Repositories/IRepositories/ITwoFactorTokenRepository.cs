using EImece.Domain.Entities;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITwoFactorTokenRepository
    {
        Task RemoveUnusedByUserIdAsync(string userId);

        Task AddAsync(TwoFactorToken token);

        Task<TwoFactorToken> FindUnusedByTokenAsync(string token);

        Task DeleteAsync(TwoFactorToken token);

        Task DeleteExpiredAndUsedAsync();

        Task<int> SaveChangesAsync();
    }
}
