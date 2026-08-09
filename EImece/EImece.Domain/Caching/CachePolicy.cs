using System;

namespace EImece.Domain.Caching
{
    /// <summary>
    /// Explicit expiration policy for <see cref="IEimeceCacheProvider"/> writes.
    /// Duration is always in seconds to match <see cref="AppConfig"/> cache settings.
    /// </summary>
    public sealed class CachePolicy
    {
        public int DurationSeconds { get; }
        public CacheExpirationMode Mode { get; }

        private CachePolicy(int durationSeconds, CacheExpirationMode mode)
        {
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be positive.");
            }

            DurationSeconds = durationSeconds;
            Mode = mode;
        }

        public static CachePolicy Absolute(int durationSeconds)
        {
            return new CachePolicy(durationSeconds, CacheExpirationMode.Absolute);
        }

        public static CachePolicy Sliding(int durationSeconds)
        {
            return new CachePolicy(durationSeconds, CacheExpirationMode.Sliding);
        }
    }
}
