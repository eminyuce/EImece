using System;
using System.Collections.Generic;
using System.Web;

namespace EImece.Domain.Helpers
{
    public class UserRoleHelper
    {
        public static bool IsDeletedEnableRoles()
        {
            var user = HttpContext.Current.User;
            var roles = GetDeletedRoles();
            foreach (var role in roles)
            {
                var r = user.IsInRole(Constants.AdministratorRole);
                if (r)
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