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
                var r = user.IsInRole(ApplicationConfigs.AdministratorRole);
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
            return user.IsInRole(ApplicationConfigs.AdministratorRole);
        }

        public static string[] GetDeletedRoles()
        {
            var roles = new List<String>();
            roles.Add(ApplicationConfigs.AdministratorRole);
            return roles.ToArray();
        }
    }
}
