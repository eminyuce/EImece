using EImece.Domain.DbContext;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    /// <summary>
    /// The only layer allowed to query ApplicationUser/IdentityRole persistence (ApplicationDbContext).
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRepository(ApplicationDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        [Timed("repo.users.get_by_id_sync")]
        public virtual ApplicationUser GetById(string id)
        {
            return _db.Users.FirstOrDefault(u => u.Id == id);
        }

        [Timed("repo.users.get_by_id")]
        public virtual Task<ApplicationUser> GetByIdAsync(string id)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        [Timed("repo.users.get_by_email_or_user_name")]
        public virtual Task<ApplicationUser> GetByEmailOrUserNameAsync(string emailOrUserName)
        {
            var key = emailOrUserName.Trim();
            return _db.Users.FirstOrDefaultAsync(u => u.UserName == key || u.Email == key);
        }

        [Timed("repo.users.is_in_role_sync")]
        public virtual bool IsUserInRole(string emailOrUserName, string roleName)
        {
            var login = emailOrUserName.Trim();
            var query = from u in _db.Users
                        from ur in u.Roles
                        join r in _db.Roles on ur.RoleId equals r.Id
                        where (u.UserName == login || u.Email == login)
                              && r.Name == roleName
                        select r.Id;

            return query.Any();
        }

        [Timed("repo.users.is_in_role")]
        public virtual async Task<bool> IsUserInRoleAsync(string emailOrUserName, string roleName)
        {
            var login = emailOrUserName.Trim();
            var query = from u in _db.Users
                        from ur in u.Roles
                        join r in _db.Roles on ur.RoleId equals r.Id
                        where (u.UserName == login || u.Email == login)
                              && r.Name == roleName
                        select r.Id;

            return await query.AnyAsync().ConfigureAwait(false);
        }

        [Timed("repo.users.get_users_filtered_sync")]
        public virtual List<ApplicationUser> GetUsersFiltered(string search)
        {
            var users = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var key = search.ToLower().Trim();
                users = users.Where(r => r.Email.ToLower().Contains(key)
                                      || r.UserName.ToLower().Contains(key)
                                      || r.FirstName.ToLower().Contains(key)
                                      || r.LastName.ToLower().Contains(key));
            }

            return users.ToList();
        }

        [Timed("repo.users.get_users_filtered")]
        public virtual async Task<List<ApplicationUser>> GetUsersFilteredAsync(string search)
        {
            var users = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var key = search.ToLower().Trim();
                users = users.Where(r => r.Email.ToLower().Contains(key)
                                      || r.UserName.ToLower().Contains(key)
                                      || r.FirstName.ToLower().Contains(key)
                                      || r.LastName.ToLower().Contains(key));
            }

            return await users.ToListAsync().ConfigureAwait(false);
        }

        [Timed("repo.users.get_first_role_name_by_user_id_sync")]
        public virtual Dictionary<string, string> GetFirstRoleNameByUserId()
        {
            var pairs = BuildUserRolePairsQuery().ToList();
            return ToFirstRoleNameMap(pairs);
        }

        [Timed("repo.users.get_first_role_name_by_user_id")]
        public virtual async Task<Dictionary<string, string>> GetFirstRoleNameByUserIdAsync()
        {
            var pairs = await BuildUserRolePairsQuery().ToListAsync().ConfigureAwait(false);
            return ToFirstRoleNameMap(pairs);
        }

        [Timed("repo.users.search_emails")]
        public virtual async Task<List<string>> SearchUserEmailsAsync(string searchKey)
        {
            var users = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                var key = searchKey.ToLower().Trim();
                users = users.Where(r => r.Email.ToLower().Contains(key)
                                      || r.UserName.ToLower().Contains(key)
                                      || r.FirstName.ToLower().Contains(key)
                                      || r.LastName.ToLower().Contains(key));
            }

            return await users.Select(r => r.Email).ToListAsync().ConfigureAwait(false);
        }

        [Timed("repo.users.delete_sync")]
        public virtual void Delete(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _db.Users.Remove(user);
            _db.SaveChanges();
        }

        [Timed("repo.users.delete")]
        public virtual async Task DeleteAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        [Timed("repo.users.update")]
        public virtual async Task UpdateAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        [Timed("repo.users.get_all_roles")]
        public virtual Task<List<IdentityRole>> GetAllRolesAsync()
        {
            return _db.Roles.ToListAsync();
        }

        [Timed("repo.users.get_users_count")]
        public virtual Task<int> GetUsersCountAsync(CancellationToken ct)
        {
            return _db.Users.CountAsync(ct);
        }

        [Timed("repo.users.get_roles_count")]
        public virtual Task<int> GetRolesCountAsync(CancellationToken ct)
        {
            return _db.Roles.CountAsync(ct);
        }

        [Timed("repo.users.get_users_paged")]
        public virtual Task<List<ApplicationUser>> GetUsersPagedAsync(int skip, int take, CancellationToken ct)
        {
            return _db.Users
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        [Timed("repo.users.get_roles_paged")]
        public virtual Task<List<IdentityRole>> GetRolesPagedAsync(int skip, int take, CancellationToken ct)
        {
            return _db.Roles
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        [Timed("repo.users.get_role_names_by_ids")]
        public virtual Task<List<string>> GetRoleNamesByIdsAsync(List<string> roleIds, CancellationToken ct)
        {
            return _db.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync(ct);
        }

        private IQueryable<UserRolePair> BuildUserRolePairsQuery()
        {
            return from u in _db.Users
                   from ur in u.Roles
                   join r in _db.Roles on ur.RoleId equals r.Id
                   select new UserRolePair { UserId = u.Id, RoleName = r.Name };
        }

        private static Dictionary<string, string> ToFirstRoleNameMap(IEnumerable<UserRolePair> pairs)
        {
            var map = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var pair in pairs)
            {
                if (!map.ContainsKey(pair.UserId))
                {
                    map.Add(pair.UserId, pair.RoleName);
                }
            }

            return map;
        }

        private sealed class UserRolePair
        {
            public string UserId { get; set; }
            public string RoleName { get; set; }
        }
    }
}
