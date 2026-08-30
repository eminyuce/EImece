using EImece.Domain.DependencyInjection;
using Resources;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers
{
    public static class AdminAuditHelper
    {
        public static string GetUserDisplayName(string userIdOrName)
        {
            if (string.IsNullOrWhiteSpace(userIdOrName))
            {
                return AdminResource.UnknownUser;
            }

            try
            {
                var usersService = DomainServiceProvider.GetService<EImece.Domain.Services.IServices.IUsersService>();
                if (usersService == null)
                {
                    return userIdOrName;
                }

                var user = ResolveUser(usersService, userIdOrName);
                var displayName = ResolveDisplayName(user);
                return displayName ?? userIdOrName;
            }
            catch
            {
                return string.IsNullOrWhiteSpace(userIdOrName) ? AdminResource.UnknownUser : userIdOrName;
            }
        }

        private static EImece.Domain.Services.ApplicationUser ResolveUser(EImece.Domain.Services.IServices.IUsersService usersService, string userIdOrName)
        {
            var user = TryGetUserById(usersService, userIdOrName);
            if (user != null) return user;

            user = TryGetUserByEmailOrUserName(usersService, userIdOrName);
            if (user != null) return user;

            return TryGetUserAsync(usersService, userIdOrName);
        }

        private static EImece.Domain.Services.ApplicationUser TryGetUserById(EImece.Domain.Services.IServices.IUsersService usersService, string userIdOrName)
        {
            try { return usersService.GetUser(userIdOrName); }
            catch { return null; }
        }

        private static EImece.Domain.Services.ApplicationUser TryGetUserByEmailOrUserName(EImece.Domain.Services.IServices.IUsersService usersService, string userIdOrName)
        {
            try { return Task.Run(() => usersService.GetUserByEmailOrUserNameAsync(userIdOrName)).GetAwaiter().GetResult(); }
            catch { return null; }
        }

        private static EImece.Domain.Services.ApplicationUser TryGetUserAsync(EImece.Domain.Services.IServices.IUsersService usersService, string userIdOrName)
        {
            try { return Task.Run(() => usersService.GetUserAsync(userIdOrName)).GetAwaiter().GetResult(); }
            catch { return null; }
        }

        private static string ResolveDisplayName(EImece.Domain.Services.ApplicationUser user)
        {
            if (user == null) return null;

            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            if (!string.IsNullOrWhiteSpace(fullName)) return fullName;
            if (!string.IsNullOrWhiteSpace(user.Email)) return user.Email;
            if (!string.IsNullOrWhiteSpace(user.UserName)) return user.UserName;
            return null;
        }

        public static string FormatAuditDate(DateTime date)
        {
            if (date == default(DateTime))
            {
                return "-";
            }

            try
            {
                var culture = System.Globalization.CultureInfo.CurrentCulture;
                // Use general date short time pattern per culture (g)
                return date.ToString("g", culture);
            }
            catch
            {
                return date.ToString();
            }
        }
    }
}
