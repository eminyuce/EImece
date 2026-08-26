using System;
using System.Collections.Generic;
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
            foreach (var role in roles)
            {
                if (user.IsInRole(role))
                {
                    return true;
                }
            }
            return false;
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