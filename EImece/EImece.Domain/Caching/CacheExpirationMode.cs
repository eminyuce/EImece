namespace EImece.Domain.Caching
{
    /// <summary>
    /// Controls how a cached entry expires. See docs/PERFORMANCE_AND_CACHING.md for when to prefer each mode.
    /// </summary>
    public enum CacheExpirationMode
    {
        /// <summary>
        /// Entry expires at a fixed wall-clock time after insert. Prefer for product catalogues,
        /// search pages, and any data that must become consistent again within a bounded window
        /// even if it keeps being read.
        /// </summary>
        Absolute = 0,

        /// <summary>
        /// Entry expires after a quiet period with no reads. Prefer for session-like or rarely
        /// changing lookups that stay warm under continuous traffic (e.g. settings, address trees).
        /// </summary>
        Sliding = 1
    }
}
