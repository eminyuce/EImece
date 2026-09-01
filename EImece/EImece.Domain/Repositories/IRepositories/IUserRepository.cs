using EImece.Domain.Services;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    /// <summary>
    /// Data access for ApplicationUser and IdentityRole records.
    /// Wraps ApplicationDbContext; services must depend on this interface instead of any DbContext.
    /// </summary>
    public interface IUserRepository
    {
        ApplicationUser GetById(string id);

        Task<ApplicationUser> GetByIdAsync(string id);

        Task<ApplicationUser> GetByEmailOrUserNameAsync(string emailOrUserName);

        bool IsUserInRole(string emailOrUserName, string roleName);

        Task<bool> IsUserInRoleAsync(string emailOrUserName, string roleName);

        List<ApplicationUser> GetUsersFiltered(string search);

        Task<List<ApplicationUser>> GetUsersFilteredAsync(string search);

        Dictionary<string, string> GetFirstRoleNameByUserId();

        Task<Dictionary<string, string>> GetFirstRoleNameByUserIdAsync();

        Task<List<string>> SearchUserEmailsAsync(string searchKey);

        void Delete(ApplicationUser user);

        Task DeleteAsync(ApplicationUser user);

        Task UpdateAsync(ApplicationUser user);

        Task<List<IdentityRole>> GetAllRolesAsync();

        Task<int> GetUsersCountAsync(CancellationToken ct);

        Task<int> GetRolesCountAsync(CancellationToken ct);

        Task<List<ApplicationUser>> GetUsersPagedAsync(int skip, int take, CancellationToken ct);

        Task<List<IdentityRole>> GetRolesPagedAsync(int skip, int take, CancellationToken ct);

        Task<List<string>> GetRoleNamesByIdsAsync(List<string> roleIds, CancellationToken ct);
    }
}
