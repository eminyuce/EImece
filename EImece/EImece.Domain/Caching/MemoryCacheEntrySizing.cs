using Microsoft.Extensions.Caching.Memory;

namespace EImece.Domain.Caching
{
    /// <summary>
    /// When <see cref="MemoryCacheOptions.SizeLimit"/> is set, every entry must specify <see cref="ICacheEntry.Size"/>.
    /// LazyCache's provider does not set size; we always assign a unit size so a future SizeLimit cannot 500 requests.
    /// </summary>
    internal static class MemoryCacheEntrySizing
    {
        internal const long DefaultEntrySize = 1;

        internal static void Apply(ICacheEntry entry)
        {
            if (entry != null)
            {
                entry.Size = DefaultEntrySize;
            }
        }

        internal static void Apply(MemoryCacheEntryOptions options)
        {
            if (options != null)
            {
                options.Size = DefaultEntrySize;
            }
        }
    }
}
