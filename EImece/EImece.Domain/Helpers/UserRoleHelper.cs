using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EImece.Domain.Helpers
{
    public class UserRoleHelper
    {
        public static bool IsDeletedEnableRoles()
        {
            var user = HttpContext.Current?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            var roles = GetDeletedRoles();
            return roles.Any(role => user.IsInRole(role));
        }

        public static bool IsAdminManagementRoles()
        {
            var user = HttpContext.Current.User;
            return user.IsInRole(Constants.AdministratorRole);
        }

        public static string[] GetDeletedRoles()
        {
            var roles = new List<String>();
            roles.Add(Constants.AdministratorRole);
            return roles.ToArray();
        }
    }
}