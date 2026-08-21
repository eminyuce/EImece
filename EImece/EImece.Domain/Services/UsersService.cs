using EImece.Domain.DbContext;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using EImece.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class UsersService : IUsersService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ApplicationDbContext _dbContext;

        public UsersService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [Inject]
        public ApplicationSignInManager SignInManager { get; set; }

        [Inject]
        public ApplicationUserManager UserManager { get; set; }

        [Inject]
        public IdentityManager IdentityManager { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        public ApplicationUser GetUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("userId should have value");
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                Logger.Debug("User is null for userId " + id);
            }
            return user;
        }

        public async Task<ApplicationUser> GetUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("userId should have value");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id).ConfigureAwait(false);
            if (user == null)
            {
                Logger.Debug("User is null for userId " + id);
            }
            return user;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var key = email.Trim();
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == key || u.Email == key).ConfigureAwait(false);
        }

        public async Task<ApplicationUser> GetUserByEmailOrUserNameAsync(string emailOrUserName)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserName))
            {
                return null;
            }

            var key = emailOrUserName.Trim();
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == key || u.Email == key).ConfigureAwait(false);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id).ConfigureAwait(false);
            return user;
        }

        public ApplicationUser GetUserById(string id)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            return user;
        }

        public async Task<bool> IsUserInRoleAsync(string emailOrUserName, string roleName)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserName) || string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }

            var login = emailOrUserName.Trim();
            var query = from u in _dbContext.Users
                        from ur in u.Roles
                        join r in _dbContext.Roles on ur.RoleId equals r.Id
                        where (u.UserName == login || u.Email == login)
                              && r.Name == roleName
                        select r.Id;

            return await query.AnyAsync().ConfigureAwait(false);
        }

        public bool IsUserInRole(string emailOrUserName, string roleName)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserName) || string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }

            var login = emailOrUserName.Trim();
            var query = from u in _dbContext.Users
                        from ur in u.Roles
                        join r in _dbContext.Roles on ur.RoleId equals r.Id
                        where (u.UserName == login || u.Email == login)
                              && r.Name == roleName
                        select r.Id;

            return query.Any();
        }

        public List<EditUserViewModel> GetUsers(string search)
        {
            var users = _dbContext.Users.AsQueryable();

            var users2 = from u in _dbContext.Users
                         from ur in u.Roles
                         join r in _dbContext.Roles on ur.RoleId equals r.Id
                         select new
                         {
                             u.Id,
                             Email = u.UserName,
                             FirstName = u.FirstName,
                             LastName = u.LastName,
                             Role = r.Name,
                         };

            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower().Trim();
                users = users.Where(r => r.Email.ToLower().Contains(search) || r.FirstName.ToLower().Contains(search) || r.LastName.ToLower().Contains(search));
            }

            //ViewModel will be posted at the end of the answer
            var model = new List<EditUserViewModel>();
            foreach (var user in users.ToList())
            {
                var u = new EditUserViewModel();
                u.FirstName = user.FirstName;
                u.LastName = user.LastName;
                u.Email = user.Email;
                u.Id = user.Id;
                u.AuthenticatorEnabled = user.TwoFactorAuthenticatorEnabled;
                var p = users2.FirstOrDefault(r => r.Id.Equals(u.Id, StringComparison.InvariantCultureIgnoreCase));
                u.Role = p == null ? String.Empty : p.Role.ToStr();
                model.Add(u);
            }

            return model;
        }

        public async Task<List<EditUserViewModel>> GetUsersAsync(string search)
        {
            var users = _dbContext.Users.AsQueryable();

            var users2 = from u in _dbContext.Users
                         from ur in u.Roles
                         join r in _dbContext.Roles on ur.RoleId equals r.Id
                         select new
                         {
                             u.Id,
                             Email = u.UserName,
                             FirstName = u.FirstName,
                             LastName = u.LastName,
                             Role = r.Name,
                         };

            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower().Trim();
                users = users.Where(r => r.Email.ToLower().Contains(search) || r.FirstName.ToLower().Contains(search) || r.LastName.ToLower().Contains(search));
            }

            var model = new List<EditUserViewModel>();
            foreach (var user in await users.ToListAsync().ConfigureAwait(false))
            {
                var u = new EditUserViewModel();
                u.FirstName = user.FirstName;
                u.LastName = user.LastName;
                u.Email = user.Email;
                u.Id = user.Id;
                u.AuthenticatorEnabled = user.TwoFactorAuthenticatorEnabled;
                var p = users2.FirstOrDefault(r => r.Id.Equals(u.Id, StringComparison.InvariantCultureIgnoreCase));
                u.Role = p == null ? String.Empty : p.Role.ToStr();
                model.Add(u);
            }

            return model;
        }

        public async Task<List<string>> SearchUserEmailsAsync(string searchKey)
        {
            var users = _dbContext.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.ToLower().Trim();
                users = users.Where(r => r.Email.ToLower().Contains(searchKey)
                                      || r.FirstName.ToLower().Contains(searchKey)
                                      || r.LastName.ToLower().Contains(searchKey));
            }

            return await users.Select(r => r.Email).ToListAsync().ConfigureAwait(false);
        }

        public void DeleteUser(string id)
        {
            var user = GetUser(id);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                _dbContext.SaveChanges();
            }
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await GetUserAsync(id).ConfigureAwait(false);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<List<string>> DeleteUsersAsync(List<string> userIds, string currentUserId = null)
        {
            var deleted = new List<string>();
            if (userIds == null || userIds.Count == 0)
            {
                return deleted;
            }

            Logger.Info($"DeleteUsersAsync called for {userIds.Count} userIds");
            foreach (var userId in userIds.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(currentUserId)
                    && string.Equals(currentUserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);
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

            Logger.Info($"DeleteUsersAsync finished. Total deleted: {deleted.Count}");
            return deleted;
        }

        public async Task UpdateUserAsync(EditUserViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var user = await _dbContext.Users.FirstAsync(u => u.Id == model.Id).ConfigureAwait(false);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;

            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<SelectUserRolesViewModel> GetAdminUserRolesViewModelAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("userId cannot be empty", nameof(userId));
            }

            var user = await _dbContext.Users.FirstAsync(u => u.Id == userId).ConfigureAwait(false);
            var allRoles = await _dbContext.Roles.ToListAsync().ConfigureAwait(false);
            var model = new SelectUserRolesViewModel(user);
            model.PopulateAdminRoles(user, allRoles);
            return model;
        }

        public async Task<SelectUserRolesViewModel> GetUserRolesViewModelAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("userId cannot be empty", nameof(userId));
            }

            var user = await _dbContext.Users.FirstAsync(u => u.Id == userId).ConfigureAwait(false);
            var allRoles = await _dbContext.Roles.ToListAsync().ConfigureAwait(false);
            var model = new SelectUserRolesViewModel(user);
            model.PopulateRoles(user, allRoles);
            return model;
        }

        public async Task<int> GetUsersCountAsync(System.Threading.CancellationToken ct = default)
        {
            return await _dbContext.Users.CountAsync(ct).ConfigureAwait(false);
        }

        public async Task<int> GetRolesCountAsync(System.Threading.CancellationToken ct = default)
        {
            return await _dbContext.Roles.CountAsync(ct).ConfigureAwait(false);
        }

        public async Task<List<EImece.Domain.Services.ExportImport.UserExportDto>> GetUsersForExportAsync(int skip, int take, System.Threading.CancellationToken ct = default)
        {
            var query = _dbContext.Users.AsNoTracking().OrderBy(x => x.Id).Skip(skip).Take(take);
            var items = await query.ToListAsync(ct).ConfigureAwait(false);

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
                    var roleNames = await _dbContext.Roles
                        .Where(r => roleIds.Contains(r.Id))
                        .Select(r => r.Name)
                        .ToListAsync(ct)
                        .ConfigureAwait(false);
                    dto.Roles = roleNames;
                }

                userDtos.Add(dto);
            }

            return userDtos;
        }

        public async Task<List<EImece.Domain.Services.ExportImport.RoleExportDto>> GetRolesForExportAsync(int skip, int take, System.Threading.CancellationToken ct = default)
        {
            var query = _dbContext.Roles.AsNoTracking().OrderBy(x => x.Id).Skip(skip).Take(take);
            var items = await query.ToListAsync(ct).ConfigureAwait(false);
            return items.Select(x => new EImece.Domain.Services.ExportImport.RoleExportDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }
    }
}