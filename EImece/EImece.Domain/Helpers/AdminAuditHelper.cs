using Resources;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

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
                var usersService = DependencyResolver.Current.GetService<EImece.Domain.Services.IServices.IUsersService>();
                if (usersService == null)
                {
                    return userIdOrName;
                }

                EImece.Domain.Services.ApplicationUser user = null;

                // Try by Id first (sync)
                try
                {
                    user = usersService.GetUser(userIdOrName);
                }
                catch
                {
                    // ignore
                }

                // Try by email / userName via async helper (blocking)
                if (user == null)
                {
                    try
                    {
                        user = Task.Run(() => usersService.GetUserByEmailOrUserNameAsync(userIdOrName)).GetAwaiter().GetResult();
                    }
                    catch
                    {
                        // ignore
                    }
                }

                // Try by Id async as fallback
                if (user == null)
                {
                    try
                    {
                        user = Task.Run(() => usersService.GetUserAsync(userIdOrName)).GetAwaiter().GetResult();
                    }
                    catch
                    {
                        // ignore
                    }
                }

                if (user != null)
                {
                    var fullName = $"{user.FirstName} {user.LastName}".Trim();
                    if (!string.IsNullOrWhiteSpace(fullName))
                    {
                        return fullName;
                    }
                    if (!string.IsNullOrWhiteSpace(user.Email))
                    {
                        return user.Email;
                    }
                    if (!string.IsNullOrWhiteSpace(user.UserName))
                    {
                        return user.UserName;
                    }
                }

                return userIdOrName;
            }
            catch
            {
                return string.IsNullOrWhiteSpace(userIdOrName) ? AdminResource.UnknownUser : userIdOrName;
            }
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
