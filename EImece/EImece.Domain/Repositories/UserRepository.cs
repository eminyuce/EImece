using EImece.Domain.DbContext;
using EImece.Domain.Entities;
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

        public ApplicationUser GetById(string id)
        {
            return _db.Users.FirstOrDefault(u => u.Id == id);
        }

        public Task<ApplicationUser> GetByIdAsync(string id)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public Task<ApplicationUser> GetByEmailOrUserNameAsync(string emailOrUserName)
        {
            var key = emailOrUserName.Trim();
            return _db.Users.FirstOrDefaultAsync(u => u.UserName == key || u.Email == key);
        }

        public bool IsUserInRole(string emailOrUserName, string roleName)
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

        public async Task<bool> IsUserInRoleAsync(string emailOrUserName, string roleName)
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

        public List<ApplicationUser> GetUsersFiltered(string search)
        {
            var users = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var key = search.ToLower().Trim();
                users = users.Where(r => r.Email.ToLower().Contains(key)
                                      || r.FirstName.ToLower().Contains(key)
                                      || r.LastName.ToLower().Contains(key));
            }

            return users.ToList();
        }

        public async Task<List<ApplicationUser>> GetUsersFilteredAsync(string search)
        {
            var users = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var key = search.ToLower().Trim();
                users = users.Where(r => r.Email.ToLower().Contains(key)
                                      || r.FirstName.ToLower().Contains(key)
                                      || r.LastName.ToLower().Contains(key));
            }

            return await users.ToListAsync().ConfigureAwait(false);
        }

        public Dictionary<string, string> GetFirstRoleNameByUserId()
        {
            var pairs = BuildUserRolePairsQuery().ToList();
            return ToFirstRoleNameMap(pairs);
        }

        public async Task<Dictionary<string, string>> GetFirstRoleNameByUserIdAsync()
        {
            var pairs = await BuildUserRolePairsQuery().ToListAsync().ConfigureAwait(false);
            return ToFirstRoleNameMap(pairs);
        }

        public async Task<List<string>> SearchUserEmailsAsync(string searchKey)
        {
            var users = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                var key = searchKey.ToLower().Trim();
                users = users.Where(r => r.Email.ToLower().Contains(key)
                                      || r.FirstName.ToLower().Contains(key)
                                      || r.LastName.ToLower().Contains(key));
            }

            return await users.Select(r => r.Email).ToListAsync().ConfigureAwait(false);
        }

        public void Delete(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _db.Users.Remove(user);
            _db.SaveChanges();
        }

        public async Task DeleteAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<List<IdentityRole>> GetAllRolesAsync()
        {
            return _db.Roles.ToListAsync();
        }

        public Task<int> GetUsersCountAsync(CancellationToken ct)
        {
            return _db.Users.CountAsync(ct);
        }

        public Task<int> GetRolesCountAsync(CancellationToken ct)
        {
            return _db.Roles.CountAsync(ct);
        }

        public Task<List<ApplicationUser>> GetUsersPagedAsync(int skip, int take, CancellationToken ct)
        {
            return _db.Users
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public Task<List<IdentityRole>> GetRolesPagedAsync(int skip, int take, CancellationToken ct)
        {
            return _db.Roles
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public Task<List<string>> GetRoleNamesByIdsAsync(List<string> roleIds, CancellationToken ct)
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
