using EImece.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Helpers
{
    public static class UserAdminFilterHelper
    {
        public static IEnumerable<EditUserViewModel> Apply(IEnumerable<EditUserViewModel> users, string role, string twoFactor, string locked)
        {
            if (users == null)
            {
                return Enumerable.Empty<EditUserViewModel>();
            }

            IEnumerable<EditUserViewModel> query = users;

            if (!string.IsNullOrWhiteSpace(role))
            {
                var roleKey = role.Trim();
                query = query.Where(r => string.Equals(r.Role, roleKey, StringComparison.OrdinalIgnoreCase));
            }

            if (IsOn(twoFactor))
            {
                query = query.Where(r => r.AuthenticatorEnabled);
            }
            else if (IsOff(twoFactor))
            {
                query = query.Where(r => !r.AuthenticatorEnabled);
            }

            if (IsOn(locked))
            {
                query = query.Where(r => r.IsLockedOut);
            }
            else if (IsOff(locked))
            {
                query = query.Where(r => !r.IsLockedOut);
            }

            return query;
        }

        public static bool IsOn(string value)
        {
            return string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsOff(string value)
        {
            return string.Equals(value, "no", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "0", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
        }
    }
}
