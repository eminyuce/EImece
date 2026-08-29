using System;

namespace EImece.Domain.Helpers
{
    public static class UserLockoutHelper
    {
        public const int DefaultLockoutMinutes = 5;

        public static bool IsLockedOut(DateTime? lockoutEndUtc, DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            return lockoutEndUtc.HasValue && lockoutEndUtc.Value > now;
        }

        public static int RemainingMinutes(DateTimeOffset lockoutEndUtc, DateTimeOffset? utcNow = null)
        {
            var now = utcNow ?? DateTimeOffset.UtcNow;
            var remaining = lockoutEndUtc - now;
            if (remaining.TotalSeconds <= 0)
            {
                return 1;
            }

            return Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
        }

        public static int RemainingSeconds(DateTimeOffset lockoutEndUtc, DateTimeOffset? utcNow = null)
        {
            var now = utcNow ?? DateTimeOffset.UtcNow;
            var remaining = lockoutEndUtc - now;
            if (remaining.TotalSeconds <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(remaining.TotalSeconds);
        }
    }
}
