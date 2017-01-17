using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                var r = user.IsInRole(Settings.AdministratorRole);
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
            return user.IsInRole(Settings.AdministratorRole);
        }

        public static string[] GetDeletedRoles()
        {
            var roles = new List<String>();
            roles.Add(Settings.AdministratorRole);
            return roles.ToArray();
        }
    }
}
