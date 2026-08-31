using Microsoft.Extensions.Logging;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class UsersService : IUsersService
    {
        private readonly ILogger<UsersService> _logger;

        private readonly IUserRepository _userRepository;
        public ApplicationUserManager UserManager { get; }
        private readonly ICustomerService CustomerService;
        private readonly IOrderService OrderService;

        public UsersService(IUserRepository userRepository,
            ApplicationUserManager userManager,
            ICustomerService customerService,
            IOrderService orderService, ILogger<UsersService> logger)
         {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            CustomerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            OrderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        [Timed("service.users.get_by_id_sync")]
        public virtual ApplicationUser GetUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("userId should have value");
            }

            var user = _userRepository.GetById(id);
            if (user == null)
            {
                _logger.LogDebug("User is null for userId " + id);
            }
            return user;
        }

        [Timed("service.users.get_by_id")]
        public virtual async Task<ApplicationUser> GetUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("userId should have value");
            }

            var user = await _userRepository.GetByIdAsync(id).ConfigureAwait(false);
            if (user == null)
            {
                _logger.LogDebug("User is null for userId " + id);
            }
            return user;
        }

        [Timed("service.users.get_by_email")]
        public virtual async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return await _userRepository.GetByEmailOrUserNameAsync(email).ConfigureAwait(false);
        }

        [Timed("service.users.get_by_email_or_user_name")]
        public virtual async Task<ApplicationUser> GetUserByEmailOrUserNameAsync(string emailOrUserName)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserName))
            {
                return null;
            }

            return await _userRepository.GetByEmailOrUserNameAsync(emailOrUserName).ConfigureAwait(false);
        }

        [Timed("service.users.get_by_id")]
        public virtual async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id).ConfigureAwait(false);
            return user;
        }

        [Timed("service.users.get_by_id_sync")]
        public virtual ApplicationUser GetUserById(string id)
        {
            var user = _userRepository.GetById(id);
            return user;
        }

        [Timed("service.users.is_in_role")]
        public virtual Task<bool> IsUserInRoleAsync(string emailOrUserName, string roleName)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserName) || string.IsNullOrWhiteSpace(roleName))
            {
                return Task.FromResult(false);
            }

            return _userRepository.IsUserInRoleAsync(emailOrUserName, roleName);
        }

        [Timed("service.users.is_in_role_sync")]
        public virtual bool IsUserInRole(string emailOrUserName, string roleName)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserName) || string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }

            return _userRepository.IsUserInRole(emailOrUserName, roleName);
        }

        [Timed("service.users.get_users_sync")]
        public virtual List<EditUserViewModel> GetUsers(string search)
        {
            var users = _userRepository.GetUsersFiltered(search);
            var roleNameByUserId = _userRepository.GetFirstRoleNameByUserId();
            return BuildEditUserViewModels(users, roleNameByUserId);
        }

        [Timed("service.users.get_users")]
        public virtual async Task<List<EditUserViewModel>> GetUsersAsync(string search)
        {
            var users = await _userRepository.GetUsersFilteredAsync(search).ConfigureAwait(false);
            var roleNameByUserId = await _userRepository.GetFirstRoleNameByUserIdAsync().ConfigureAwait(false);
            return BuildEditUserViewModels(users, roleNameByUserId);
        }

        private static List<EditUserViewModel> BuildEditUserViewModels(List<ApplicationUser> users, Dictionary<string, string> roleNameByUserId)
        {
            var model = new List<EditUserViewModel>();
            foreach (var user in users)
            {
                var u = new EditUserViewModel();
                u.FirstName = user.FirstName;
                u.LastName = user.LastName;
                u.Email = user.Email;
                u.Id = user.Id;
                u.AuthenticatorEnabled = user.TwoFactorAuthenticatorEnabled;
                u.IsLockedOut = UserLockoutHelper.IsLockedOut(user.LockoutEndDateUtc);
                string role;
                u.Role = roleNameByUserId.TryGetValue(user.Id, out role) ? role.ToStr() : String.Empty;
                model.Add(u);
            }

            return model;
        }

        [Timed("service.users.search_emails")]
        public virtual Task<List<string>> SearchUserEmailsAsync(string searchKey)
        {
            return _userRepository.SearchUserEmailsAsync(searchKey);
        }

        [Timed("service.users.delete_sync")]
        public virtual void DeleteUser(string id)
        {
            var user = GetUser(id);
            if (user != null)
            {
                _userRepository.Delete(user);
            }
        }

        [Timed("service.users.delete")]
        public virtual async Task DeleteUserAsync(string id)
        {
            var user = await GetUserAsync(id).ConfigureAwait(false);
            if (user != null)
            {
                await _userRepository.DeleteAsync(user).ConfigureAwait(false);
            }
        }

        [Timed("service.users.delete_bulk")]
        public virtual async Task<List<string>> DeleteUsersAsync(List<string> userIds, string currentUserId = null)
        {
            var deleted = new List<string>();
            if (userIds == null || userIds.Count == 0)
            {
                return deleted;
            }

            _logger.LogDebug($"DeleteUsersAsync called for {userIds.Count} userIds");
            foreach (var userId in userIds.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(currentUserId)
                    && string.Equals(currentUserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var user = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
                if (user == null)
                {
                    continue;
                }

                var roles = await UserManager.GetRolesAsync(userId).ConfigureAwait(false);
                var isCustomer = roles != null
                    && roles.Any(r => r.Equals(Constants.CustomerRole, StringComparison.OrdinalIgnoreCase));
                if (!isCustomer)
                {
                    // Safety: customer grid must not bulk-delete staff accounts.
                    continue;
                }

                if (CustomerService != null)
                {
                    await CustomerService.DeleteByUserIdAsync(userId).ConfigureAwait(false);
                }
                if (OrderService != null)
                {
                    await OrderService.DeleteByUserIdAsync(userId).ConfigureAwait(false);
                }
                await DeleteUserAsync(userId).ConfigureAwait(false);
                deleted.Add(userId);
            }

            _logger.LogInformation($"DeleteUsersAsync finished. Total deleted: {deleted.Count}");
            return deleted;
        }

        [Timed("service.users.update")]
        public virtual async Task UpdateUserAsync(EditUserViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var user = await _userRepository.GetByIdAsync(model.Id).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException($"User '{model.Id}' was not found.");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;

            await _userRepository.UpdateAsync(user).ConfigureAwait(false);
        }

        [Timed("service.users.get_admin_roles_vm")]
        public virtual async Task<SelectUserRolesViewModel> GetAdminUserRolesViewModelAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("userId cannot be empty", nameof(userId));
            }

            var user = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException($"User '{userId}' was not found.");
            }

            var allRoles = await _userRepository.GetAllRolesAsync().ConfigureAwait(false);
            var model = new SelectUserRolesViewModel(user);
            model.PopulateAdminRoles(user, allRoles);
            return model;
        }

        [Timed("service.users.get_user_roles_vm")]
        public virtual async Task<SelectUserRolesViewModel> GetUserRolesViewModelAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("userId cannot be empty", nameof(userId));
            }

            var user = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException($"User '{userId}' was not found.");
            }

            var allRoles = await _userRepository.GetAllRolesAsync().ConfigureAwait(false);
            var model = new SelectUserRolesViewModel(user);
            model.PopulateRoles(user, allRoles);
            return model;
        }

        [Timed("service.users.get_users_count")]
        public virtual Task<int> GetUsersCountAsync(System.Threading.CancellationToken ct = default(CancellationToken))
        {
            return _userRepository.GetUsersCountAsync(ct);
        }

        [Timed("service.users.get_roles_count")]
        public virtual Task<int> GetRolesCountAsync(System.Threading.CancellationToken ct = default(CancellationToken))
        {
            return _userRepository.GetRolesCountAsync(ct);
        }

        [Timed("service.users.get_users_for_export")]
        public virtual async Task<List<EImece.Domain.Services.ExportImport.UserExportDto>> GetUsersForExportAsync(int skip, int take, System.Threading.CancellationToken ct = default(CancellationToken))
        {
            var items = await _userRepository.GetUsersPagedAsync(skip, take, ct).ConfigureAwait(false);

            var userDtos = new List<EImece.Domain.Services.ExportImport.UserExportDto>();
            foreach (var user in items)
            {
                var dto = new EImece.Domain.Services.ExportImport.UserExportDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TwoFactorAuthenticatorEnabled = user.TwoFactorAuthenticatorEnabled,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnabled = user.LockoutEnabled
                };

                if (user.Roles != null && user.Roles.Count > 0)
                {
                    var roleIds = user.Roles.Select(r => r.RoleId).ToList();
                    dto.Roles = await _userRepository.GetRoleNamesByIdsAsync(roleIds, ct).ConfigureAwait(false);
                }

                userDtos.Add(dto);
            }

            return userDtos;
        }

        [Timed("service.users.get_roles_for_export")]
        public virtual async Task<List<EImece.Domain.Services.ExportImport.RoleExportDto>> GetRolesForExportAsync(int skip, int take, System.Threading.CancellationToken ct = default(CancellationToken))
        {
            var items = await _userRepository.GetRolesPagedAsync(skip, take, ct).ConfigureAwait(false);
            return items.Select(x => new EImece.Domain.Services.ExportImport.RoleExportDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }
    }
}
