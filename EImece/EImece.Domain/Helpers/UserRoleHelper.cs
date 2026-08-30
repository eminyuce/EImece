using EImece.Domain.Abstractions;
using EImece.Domain.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Helpers
{
    public class UserRoleHelper
    {
        public static bool IsDeletedEnableRoles()
        {
            var userContext = DomainServiceProvider.GetService<ICurrentUserContext>();
            if (userContext == null || !userContext.IsAuthenticated)
            {
                return false;
            }

            var roles = GetDeletedRoles();
            return roles.Any(role => userContext.IsInRole(role));
        }

        public static bool IsAdminManagementRoles()
        {
            var userContext = DomainServiceProvider.GetService<ICurrentUserContext>();
            return userContext?.IsInRole(Constants.AdministratorRole) ?? false;
        }

        public static string[] GetDeletedRoles()
        {
            var roles = new List<string>();
            roles.Add(Constants.AdministratorRole);
            return roles.ToArray();
        }
    }
}