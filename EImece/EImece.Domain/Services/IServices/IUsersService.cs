using EImece.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IUsersService
    {
        ApplicationUserManager UserManager { get; }

        ApplicationUser GetUser(string id);

        Task<ApplicationUser> GetUserAsync(string id);

        Task<ApplicationUser> GetUserByEmailAsync(string email);

        Task<ApplicationUser> GetUserByEmailOrUserNameAsync(string emailOrUserName);

        Task<bool> IsUserInRoleAsync(string emailOrUserName, string roleName);

        bool IsUserInRole(string emailOrUserName, string roleName);

        List<EditUserViewModel> GetUsers(string search);

        Task<List<EditUserViewModel>> GetUsersAsync(string search);

        Task<List<string>> SearchUserEmailsAsync(string searchKey);

        void DeleteUser(string id);

        Task DeleteUserAsync(string id);

        Task<List<string>> DeleteUsersAsync(List<string> userIds, string currentUserId = null);

        Task UpdateUserAsync(EditUserViewModel model);

        Task<SelectUserRolesViewModel> GetAdminUserRolesViewModelAsync(string userId);

        Task<SelectUserRolesViewModel> GetUserRolesViewModelAsync(string userId);

        Task<int> GetUsersCountAsync(System.Threading.CancellationToken ct = default);

        Task<int> GetRolesCountAsync(System.Threading.CancellationToken ct = default);

        Task<List<EImece.Domain.Services.ExportImport.UserExportDto>> GetUsersForExportAsync(int skip, int take, System.Threading.CancellationToken ct = default);

        Task<List<EImece.Domain.Services.ExportImport.RoleExportDto>> GetRolesForExportAsync(int skip, int take, System.Threading.CancellationToken ct = default);
    }
}
