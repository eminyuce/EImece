using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace EImece.Domain.Caching
{
    /// <summary>
    /// Process-wide cache metrics and lightweight key metadata for admin diagnostics.
    /// Does not store cached values. Instrumented from <see cref="IEimeceCacheProvider"/> implementations.
    /// </summary>
    public static class CacheDiagnostics
    {
        public const string NotAvailable = "N/A";
        public const string StatusActive = "Active";
        public const string StatusExpired = "Expired";
        internal const string PhysicalPrefix = "Memory:";

        private const int MaxTrackedEntries = 5000;

        public const string KindApplicationData = "data";
        public const string KindPageResponse = "page";

        private static long _hits;
        private static long _misses;
        private static long _sets;
        private static long _removals;
        private static long _expirations;
        private static long _cachedTicks;
        private static long _cachedSamples;
        private static long _uncachedTicks;
        private static long _uncachedSamples;
        private static long _outputHits;
        private static long _outputMisses;
        private static long _outputCachedTicks;
        private static long _outputCachedSamples;
        private static long _outputUncachedTicks;
        private static long _outputUncachedSamples;

        private static readonly ConcurrentDictionary<string, CacheEntryTracker> Entries =
            new ConcurrentDictionary<string, CacheEntryTracker>(StringComparer.Ordinal);

        public static long Hits => Interlocked.Read(ref _hits);
        public static long Misses => Interlocked.Read(ref _misses);
        public static long Sets => Interlocked.Read(ref _sets);
        public static long Removals => Interlocked.Read(ref _removals);
        public static long Expirations => Interlocked.Read(ref _expirations);
        public static long OutputHits => Interlocked.Read(ref _outputHits);
        public static long OutputMisses => Interlocked.Read(ref _outputMisses);

        public static double HitRatioPercent
        {
            get
            {
                var hits = Hits;
                var misses = Misses;
                var total = hits + misses;
                if (total <= 0)
                {
                    return 0d;
                }

                return Math.Round((hits / (double)total) * 100d, 2, MidpointRounding.AwayFromZero);
            }
        }

        public static void Reset()
        {
            Interlocked.Exchange(ref _hits, 0);
            Interlocked.Exchange(ref _misses, 0);
            Interlocked.Exchange(ref _sets, 0);
            Interlocked.Exchange(ref _removals, 0);
            Interlocked.Exchange(ref _expirations, 0);
            Interlocked.Exchange(ref _cachedTicks, 0);
            Interlocked.Exchange(ref _cachedSamples, 0);
            Interlocked.Exchange(ref _uncachedTicks, 0);
            Interlocked.Exchange(ref _uncachedSamples, 0);
            Interlocked.Exchange(ref _outputHits, 0);
            Interlocked.Exchange(ref _outputMisses, 0);
            Interlocked.Exchange(ref _outputCachedTicks, 0);
            Interlocked.Exchange(ref _outputCachedSamples, 0);
            Interlocked.Exchange(ref _outputUncachedTicks, 0);
            Interlocked.Exchange(ref _outputUncachedSamples, 0);
            Entries.Clear();
        }

        /// <summary>
        /// Records elapsed ticks for a data-cache lookup. Call after hit/miss/set so the key exists.
        /// Uncached duration is only meaningful for GetOrAdd (factory time). Do not invent it for Get() misses.
        /// </summary>
        public static void RecordLookupDuration(string physicalKey, bool isHit, long ticks)
        {
            if (ticks < 0)
            {
                return;
            }

            if (isHit)
            {
                Interlocked.Add(ref _cachedTicks, ticks);
                Interlocked.Increment(ref _cachedSamples);
            }
            else
            {
                Interlocked.Add(ref _uncachedTicks, ticks);
                Interlocked.Increment(ref _uncachedSamples);
            }

            CacheEntryTracker tracker;
            if (string.IsNullOrEmpty(physicalKey) || !Entries.TryGetValue(physicalKey, out tracker))
            {
                return;
            }

            if (isHit)
            {
                Interlocked.Add(ref tracker.CachedTicks, ticks);
                Interlocked.Increment(ref tracker.CachedSamples);
            }
            else
            {
                Interlocked.Add(ref tracker.UncachedTicks, ticks);
                Interlocked.Increment(ref tracker.UncachedSamples);
            }
        }

        public static void RecordOutputHit(long ticks)
        {
            Interlocked.Increment(ref _outputHits);
            if (ticks >= 0)
            {
                Interlocked.Add(ref _outputCachedTicks, ticks);
                Interlocked.Increment(ref _outputCachedSamples);
            }
        }

        public static void RecordOutputMiss(long ticks)
        {
            Interlocked.Increment(ref _outputMisses);
            if (ticks >= 0)
            {
                Interlocked.Add(ref _outputUncachedTicks, ticks);
                Interlocked.Increment(ref _outputUncachedSamples);
            }
        }

        public static void RecordHit(string physicalKey)
        {
            Interlocked.Increment(ref _hits);
            CacheEntryTracker tracker;
            if (!string.IsNullOrEmpty(physicalKey) && Entries.TryGetValue(physicalKey, out tracker))
            {
                Interlocked.Increment(ref tracker.HitCount);
                tracker.LastAccessUtc = DateTimeOffset.UtcNow;
            }
        }

        public static void RecordMiss(string physicalKey)
        {
            Interlocked.Increment(ref _misses);
            CacheEntryTracker tracker;
            if (!string.IsNullOrEmpty(physicalKey) && Entries.TryGetValue(physicalKey, out tracker))
            {
                Interlocked.Increment(ref tracker.MissCount);
                if (tracker.Status == 0 &&
                    tracker.AbsoluteExpirationUtc.HasValue &&
                    tracker.AbsoluteExpirationUtc.Value <= DateTimeOffset.UtcNow)
                {
                    tracker.Status = 1;
                    Interlocked.Increment(ref _expirations);
                }
            }
        }

        public static void RecordSet(string physicalKey, Type type, CachePolicy policy)
        {
            Interlocked.Increment(ref _sets);
            if (string.IsNullOrEmpty(physicalKey))
            {
                return;
            }

            PruneIfNeeded();

            var now = DateTimeOffset.UtcNow;
            var logical = ToLogicalKey(physicalKey);
            var tracker = Entries.GetOrAdd(physicalKey, _ => new CacheEntryTracker
            {
                PhysicalKey = physicalKey,
                LogicalKey = logical,
                Category = ResolveCategory(logical)
            });

            tracker.TypeName = type != null ? type.FullName : NotAvailable;
            tracker.CreatedUtc = now;
            tracker.LastAccessUtc = now;
            tracker.Status = 0;
            tracker.SlidingSeconds = null;
            tracker.AbsoluteExpirationUtc = null;

            if (policy != null)
            {
                if (policy.Mode == CacheExpirationMode.Sliding)
                {
                    tracker.SlidingSeconds = policy.DurationSeconds;
                }
                else
                {
                    tracker.AbsoluteExpirationUtc = now.AddSeconds(policy.DurationSeconds);
                }
            }
        }

        public static void RecordRemove(string physicalKey)
        {
            Interlocked.Increment(ref _removals);
            CacheEntryTracker removed;
            if (!string.IsNullOrEmpty(physicalKey))
            {
                Entries.TryRemove(physicalKey, out removed);
            }
        }

        public static void RecordExpiration(string physicalKey)
        {
            Interlocked.Increment(ref _expirations);
            CacheEntryTracker tracker;
            if (!string.IsNullOrEmpty(physicalKey) && Entries.TryGetValue(physicalKey, out tracker))
            {
                tracker.Status = 1;
            }
        }

        /// <summary>
        /// Eviction callback from the underlying provider. Distinguishes expiry from explicit remove
        /// so counters are not double-counted when <see cref="RecordRemove"/> already ran.
        /// </summary>
        public static void HandleProviderEviction(string physicalKey, bool expired)
        {
            if (string.IsNullOrEmpty(physicalKey))
            {
                return;
            }

            if (expired)
            {
                RecordExpiration(physicalKey);
                return;
            }

            CacheEntryTracker removed;
            Entries.TryRemove(physicalKey, out removed);
        }

        public static CacheMetricsSnapshot GetMetrics()
        {
            var hits = Hits;
            var misses = Misses;
            var outputHits = OutputHits;
            var outputMisses = OutputMisses;
            var cachedMs = CacheHealth.AverageMs(Interlocked.Read(ref _cachedTicks), Interlocked.Read(ref _cachedSamples));
            var uncachedMs = CacheHealth.AverageMs(Interlocked.Read(ref _uncachedTicks), Interlocked.Read(ref _uncachedSamples));
            var outputCachedMs = CacheHealth.AverageMs(Interlocked.Read(ref _outputCachedTicks), Interlocked.Read(ref _outputCachedSamples));
            var outputUncachedMs = CacheHealth.AverageMs(Interlocked.Read(ref _outputUncachedTicks), Interlocked.Read(ref _outputUncachedSamples));
            return new CacheMetricsSnapshot
            {
                Hits = hits,
                Misses = misses,
                Sets = Sets,
                Removals = Removals,
                Expirations = Expirations,
                TotalReads = hits + misses,
                HitRatioPercent = HitRatioPercent,
                TrackedEntryCount = Entries.Count,
                AvgCachedMs = cachedMs,
                AvgUncachedMs = uncachedMs,
                OutputHits = outputHits,
                OutputMisses = outputMisses,
                OutputHitRatioPercent = CacheHealth.HitRatioPercent(outputHits, outputMisses),
                OutputAvgCachedMs = outputCachedMs,
                OutputAvgUncachedMs = outputUncachedMs
            };
        }

        public static CacheOverviewSnapshot BuildOverview()
        {
            var dataHits = Hits;
            var dataMisses = Misses;
            var dataCachedSamples = Interlocked.Read(ref _cachedSamples);
            var dataUncachedSamples = Interlocked.Read(ref _uncachedSamples);
            var dataCachedMs = CacheHealth.AverageMs(Interlocked.Read(ref _cachedTicks), dataCachedSamples);
            var dataUncachedMs = CacheHealth.AverageMs(Interlocked.Read(ref _uncachedTicks), dataUncachedSamples);
            var dataImprovement = CacheHealth.ImprovementPercent(dataUncachedMs, dataCachedMs);
            var dataEnabled = AppConfig.IsCacheActive;
            var activeEntries = 0;
            foreach (var pair in Entries)
            {
                if (pair.Value.Status == 0)
                {
                    activeEntries++;
                }
            }

            var data = new CacheLayerSnapshot
            {
                Kind = KindApplicationData,
                IsEnabled = dataEnabled,
                Hits = dataHits,
                Misses = dataMisses,
                TotalReads = dataHits + dataMisses,
                HitRatioPercent = CacheHealth.HitRatioPercent(dataHits, dataMisses),
                ActiveEntries = activeEntries,
                AvgCachedMs = dataCachedMs,
                AvgUncachedMs = dataUncachedMs,
                ImprovementPercent = dataImprovement,
                SavedMs = CacheHealth.SavedMs(dataHits, dataUncachedMs, dataCachedMs),
                HasTiming = dataCachedMs.HasValue && dataUncachedMs.HasValue,
                EstimatedDatabaseOperationsAvoided = dataHits > 0 ? dataHits : (long?)null,
                Effectiveness = CacheHealth.Evaluate(dataEnabled, dataHits, dataMisses, dataImprovement)
            };

            var pageHits = OutputHits;
            var pageMisses = OutputMisses;
            var pageCachedSamples = Interlocked.Read(ref _outputCachedSamples);
            var pageUncachedSamples = Interlocked.Read(ref _outputUncachedSamples);
            var pageCachedMs = CacheHealth.AverageMs(Interlocked.Read(ref _outputCachedTicks), pageCachedSamples);
            var pageUncachedMs = CacheHealth.AverageMs(Interlocked.Read(ref _outputUncachedTicks), pageUncachedSamples);
            var pageImprovement = CacheHealth.ImprovementPercent(pageUncachedMs, pageCachedMs);
            var page = new CacheLayerSnapshot
            {
                Kind = KindPageResponse,
                IsEnabled = true,
                Hits = pageHits,
                Misses = pageMisses,
                TotalReads = pageHits + pageMisses,
                HitRatioPercent = CacheHealth.HitRatioPercent(pageHits, pageMisses),
                ActiveEntries = 0,
                AvgCachedMs = pageCachedMs,
                AvgUncachedMs = pageUncachedMs,
                ImprovementPercent = pageImprovement,
                SavedMs = CacheHealth.SavedMs(pageHits, pageUncachedMs, pageCachedMs),
                HasTiming = pageCachedMs.HasValue && pageUncachedMs.HasValue,
                EstimatedDatabaseOperationsAvoided = null,
                Effectiveness = CacheHealth.Evaluate(true, pageHits, pageMisses, pageImprovement)
            };

            var combinedHits = dataHits + pageHits;
            var combinedMisses = dataMisses + pageMisses;
            double? combinedCachedMs;
            double? combinedUncachedMs;
            if (data.HasTiming && page.HasTiming)
            {
                combinedCachedMs = CacheHealth.WeightedAverage(dataCachedMs, dataCachedSamples, pageCachedMs, pageCachedSamples);
                combinedUncachedMs = CacheHealth.WeightedAverage(dataUncachedMs, dataUncachedSamples, pageUncachedMs, pageUncachedSamples);
            }
            else if (data.HasTiming)
            {
                combinedCachedMs = dataCachedMs;
                combinedUncachedMs = dataUncachedMs;
            }
            else if (page.HasTiming)
            {
                combinedCachedMs = pageCachedMs;
                combinedUncachedMs = pageUncachedMs;
            }
            else
            {
                combinedCachedMs = null;
                combinedUncachedMs = null;
            }

            var combinedImprovement = CacheHealth.ImprovementPercent(combinedUncachedMs, combinedCachedMs);
            double? combinedSaved = null;
            if (data.SavedMs.HasValue || page.SavedMs.HasValue)
            {
                combinedSaved = (data.SavedMs ?? 0d) + (page.SavedMs ?? 0d);
            }

            var combinedEnabled = dataEnabled || page.TotalReads > 0;
            var combined = new CacheLayerSnapshot
            {
                Kind = "combined",
                IsEnabled = combinedEnabled,
                Hits = combinedHits,
                Misses = combinedMisses,
                TotalReads = combinedHits + combinedMisses,
                HitRatioPercent = CacheHealth.HitRatioPercent(combinedHits, combinedMisses),
                ActiveEntries = activeEntries,
                AvgCachedMs = combinedCachedMs,
                AvgUncachedMs = combinedUncachedMs,
                ImprovementPercent = combinedImprovement,
                SavedMs = combinedSaved,
                HasTiming = combinedCachedMs.HasValue && combinedUncachedMs.HasValue,
                EstimatedDatabaseOperationsAvoided = data.EstimatedDatabaseOperationsAvoided,
                Effectiveness = CacheHealth.Evaluate(combinedEnabled, combinedHits, combinedMisses, combinedImprovement)
            };

            return new CacheOverviewSnapshot
            {
                IsCacheActive = dataEnabled,
                Combined = combined,
                Page = page,
                Data = data,
                PageCacheProfiles = new[] { Constants.Cache1Hour, Constants.Cache20Minutes, Constants.Cache1Day }
            };
        }

        public static CacheEntryQueryResult QueryEntries(string search, string category, string status, int page, int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 50;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var now = DateTimeOffset.UtcNow;
            var snapshots = new List<CacheEntrySnapshot>(Entries.Count);
            var categories = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in Entries)
            {
                var snapshot = ToSnapshot(pair.Value, now);
                categories.Add(snapshot.Category);
                if (!Matches(snapshot, search, category, status))
                {
                    continue;
                }

                snapshots.Add(snapshot);
            }

            snapshots.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
            var total = snapshots.Count;
            var skip = (page - 1) * pageSize;
            if (skip > total)
            {
                skip = 0;
                page = 1;
            }

            return new CacheEntryQueryResult
            {
                Entries = snapshots.Skip(skip).Take(pageSize).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                Categories = categories.ToList()
            };
        }

        /// <summary>
        /// All matching entries without pagination. For admin export only; does not load cached values.
        /// </summary>
        public static List<CacheEntrySnapshot> GetMatchingEntries(string search, string category, string status)
        {
            var now = DateTimeOffset.UtcNow;
            var snapshots = new List<CacheEntrySnapshot>(Entries.Count);

            foreach (var pair in Entries)
            {
                var snapshot = ToSnapshot(pair.Value, now);
                if (!Matches(snapshot, search, category, status))
                {
                    continue;
                }

                snapshots.Add(snapshot);
            }

            snapshots.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
            return snapshots;
        }

        public static CacheEntrySnapshot GetEntry(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var physical = key.StartsWith(PhysicalPrefix, StringComparison.Ordinal) ? key : PhysicalPrefix + key;
            CacheEntryTracker tracker;
            if (!Entries.TryGetValue(physical, out tracker) && !Entries.TryGetValue(key, out tracker))
            {
                return null;
            }

            return ToSnapshot(tracker, DateTimeOffset.UtcNow);
        }

        public static string ToLogicalKey(string physicalKey)
        {
            if (string.IsNullOrEmpty(physicalKey))
            {
                return physicalKey;
            }

            if (physicalKey.StartsWith(PhysicalPrefix, StringComparison.Ordinal))
            {
                return physicalKey.Substring(PhysicalPrefix.Length);
            }

            return physicalKey;
        }

        public static string ToDisplayName(string logicalKey)
        {
            if (string.IsNullOrEmpty(logicalKey))
            {
                return logicalKey;
            }

            var key = logicalKey;
            if (key.EndsWith(":async", StringComparison.OrdinalIgnoreCase))
            {
                key = key.Substring(0, key.Length - 6);
            }

            if (key.StartsWith(CacheKeys.ProductDetailPrefix, StringComparison.OrdinalIgnoreCase)) return "Product page";
            if (key.StartsWith(CacheKeys.ProductListPrefix, StringComparison.OrdinalIgnoreCase)) return "Product list";
            if (key.StartsWith(CacheKeys.ProductSearchPrefix, StringComparison.OrdinalIgnoreCase)) return "Product search";
            if (key.StartsWith(CacheKeys.ProductRelatedPrefix, StringComparison.OrdinalIgnoreCase)) return "Related products";
            if (key.StartsWith(CacheKeys.ProductTagPrefix, StringComparison.OrdinalIgnoreCase)) return "Products by tag";
            if (key.StartsWith(CacheKeys.CategoryTreePrefix, StringComparison.OrdinalIgnoreCase)) return "Store navigation";
            if (key.StartsWith(CacheKeys.CategoryMainPagePrefix, StringComparison.OrdinalIgnoreCase)) return "Homepage categories";
            if (key.StartsWith(CacheKeys.CategoryDetailPrefix, StringComparison.OrdinalIgnoreCase)) return "Category page";
            if (key.StartsWith(CacheKeys.CategoryPrefix, StringComparison.OrdinalIgnoreCase)) return "Categories";
            if (key.StartsWith(CacheKeys.BannerPrefix, StringComparison.OrdinalIgnoreCase)) return "Homepage banners";
            if (key.StartsWith(CacheKeys.MenuPrefix, StringComparison.OrdinalIgnoreCase)) return "Store navigation";
            if (string.Equals(key, CacheKeys.WebSiteLogoImage, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, CacheKeys.WebSiteLogoImageLegacy, StringComparison.OrdinalIgnoreCase))
            {
                return "Website logo";
            }
            if (key.StartsWith(CacheKeys.SettingPrefix, StringComparison.OrdinalIgnoreCase)) return "Settings";
            if (key.StartsWith(CacheKeys.StoryPrefix, StringComparison.OrdinalIgnoreCase)) return "Stories";
            if (key.StartsWith(CacheKeys.FaqPrefix, StringComparison.OrdinalIgnoreCase)) return "FAQ";
            if (key.StartsWith(CacheKeys.BrandPrefix, StringComparison.OrdinalIgnoreCase)) return "Brands";
            if (key.StartsWith(CacheKeys.TagPrefix, StringComparison.OrdinalIgnoreCase)) return "Tags";
            if (key.StartsWith(CacheKeys.OrderPrefix, StringComparison.OrdinalIgnoreCase)) return "Orders";
            if (key.StartsWith(CacheKeys.RssPrefix, StringComparison.OrdinalIgnoreCase)) return "RSS";
            if (key.StartsWith("filestorage", StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                return "Images";
            }

            var colon = key.IndexOf(':');
            if (colon > 0)
            {
                return ResolveCategory(key);
            }

            var underscore = key.IndexOf('_');
            if (underscore > 0)
            {
                return key.Substring(0, underscore);
            }

            return key;
        }

        public static string ResolveCategory(string logicalKey)
        {
            if (string.IsNullOrEmpty(logicalKey))
            {
                return "Other";
            }

            var area = logicalKey;
            var colon = logicalKey.IndexOf(':');
            if (colon > 0)
            {
                area = logicalKey.Substring(0, colon);
            }

            if (string.Equals(area, CacheKeys.ProductArea, StringComparison.OrdinalIgnoreCase)) return "Products";
            if (string.Equals(area, CacheKeys.CategoryArea, StringComparison.OrdinalIgnoreCase)) return "Categories";
            if (string.Equals(area, CacheKeys.SettingArea, StringComparison.OrdinalIgnoreCase)) return "Settings";
            if (string.Equals(area, CacheKeys.MenuArea, StringComparison.OrdinalIgnoreCase)) return "Menus";
            if (string.Equals(area, CacheKeys.BrandArea, StringComparison.OrdinalIgnoreCase)) return "Brands";
            if (string.Equals(area, CacheKeys.TagArea, StringComparison.OrdinalIgnoreCase)) return "Tags";
            if (string.Equals(area, CacheKeys.StoryArea, StringComparison.OrdinalIgnoreCase)) return "Stories";
            if (string.Equals(area, CacheKeys.FaqArea, StringComparison.OrdinalIgnoreCase)) return "FAQ";
            if (string.Equals(area, CacheKeys.BannerArea, StringComparison.OrdinalIgnoreCase)) return "Banners";
            if (string.Equals(area, CacheKeys.OrderArea, StringComparison.OrdinalIgnoreCase)) return "Orders";
            if (string.Equals(area, CacheKeys.RssArea, StringComparison.OrdinalIgnoreCase)) return "Rss";
            return char.ToUpperInvariant(area[0]) + area.Substring(1);
        }

        private static bool Matches(CacheEntrySnapshot snapshot, string search, string category, string status)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var keyMatch = snapshot.Key != null &&
                    snapshot.Key.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
                var nameMatch = snapshot.DisplayName != null &&
                    snapshot.DisplayName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!keyMatch && !nameMatch)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(category) &&
                !string.Equals(category, "all", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(snapshot.Category, category, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(status) &&
                !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(snapshot.Status, status, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static CacheEntrySnapshot ToSnapshot(CacheEntryTracker tracker, DateTimeOffset now)
        {
            var status = tracker.Status == 1 ? StatusExpired : StatusActive;
            if (tracker.Status == 0 && tracker.AbsoluteExpirationUtc.HasValue && tracker.AbsoluteExpirationUtc.Value <= now)
            {
                status = StatusExpired;
            }

            var hits = Interlocked.Read(ref tracker.HitCount);
            var misses = Interlocked.Read(ref tracker.MissCount);
            var avgCached = CacheHealth.AverageMs(Interlocked.Read(ref tracker.CachedTicks), Interlocked.Read(ref tracker.CachedSamples));
            var avgUncached = CacheHealth.AverageMs(Interlocked.Read(ref tracker.UncachedTicks), Interlocked.Read(ref tracker.UncachedSamples));
            var improvement = CacheHealth.ImprovementPercent(avgUncached, avgCached);
            var logical = tracker.LogicalKey;

            return new CacheEntrySnapshot
            {
                Key = logical,
                DisplayName = ToDisplayName(logical),
                CacheKind = KindApplicationData,
                Category = tracker.Category,
                Status = status,
                TypeName = string.IsNullOrEmpty(tracker.TypeName) ? NotAvailable : tracker.TypeName,
                Size = NotAvailable,
                CreatedUtc = tracker.CreatedUtc,
                ExpiresUtc = tracker.AbsoluteExpirationUtc,
                Ttl = FormatTtl(tracker, now, status),
                HitCount = hits,
                Misses = misses,
                MissCount = misses.ToString("N0", CultureInfo.InvariantCulture),
                HitRatioPercent = CacheHealth.HitRatioPercent(hits, misses),
                AvgCachedMs = avgCached,
                AvgUncachedMs = avgUncached,
                ImprovementPercent = improvement,
                LastAccessUtc = tracker.LastAccessUtc,
                SlidingSeconds = tracker.SlidingSeconds.HasValue
                    ? tracker.SlidingSeconds.Value.ToString(CultureInfo.InvariantCulture)
                    : NotAvailable
            };
        }

        private static string FormatTtl(CacheEntryTracker tracker, DateTimeOffset now, string status)
        {
            if (string.Equals(status, StatusExpired, StringComparison.OrdinalIgnoreCase))
            {
                return "0";
            }

            if (tracker.AbsoluteExpirationUtc.HasValue)
            {
                var remaining = tracker.AbsoluteExpirationUtc.Value - now;
                if (remaining <= TimeSpan.Zero)
                {
                    return "0";
                }

                return FormatTimeSpan(remaining);
            }

            if (tracker.SlidingSeconds.HasValue)
            {
                var remaining = TimeSpan.FromSeconds(tracker.SlidingSeconds.Value) - (now - tracker.LastAccessUtc);
                if (remaining <= TimeSpan.Zero)
                {
                    return "sliding";
                }

                return "sliding " + FormatTimeSpan(remaining);
            }

            return NotAvailable;
        }

        private static string FormatTimeSpan(TimeSpan value)
        {
            if (value.TotalDays >= 1)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}d {1}h", (int)value.TotalDays, value.Hours);
            }

            if (value.TotalHours >= 1)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}h {1}m", (int)value.TotalHours, value.Minutes);
            }

            if (value.TotalMinutes >= 1)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}m", (int)value.TotalMinutes);
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}s", Math.Max(1, (int)value.TotalSeconds));
        }

        private static void PruneIfNeeded()
        {
            if (Entries.Count < MaxTrackedEntries)
            {
                return;
            }

            foreach (var pair in Entries)
            {
                if (pair.Value.Status == 1)
                {
                    CacheEntryTracker removed;
                    Entries.TryRemove(pair.Key, out removed);
                }
            }
        }

        private sealed class CacheEntryTracker
        {
            public string PhysicalKey;
            public string LogicalKey;
            public string Category;
            public string TypeName;
            public DateTimeOffset CreatedUtc;
            public DateTimeOffset? AbsoluteExpirationUtc;
            public int? SlidingSeconds;
            public long HitCount;
            public long MissCount;
            public long CachedTicks;
            public long CachedSamples;
            public long UncachedTicks;
            public long UncachedSamples;
            public DateTimeOffset LastAccessUtc;
            public int Status;
        }
    }

    public sealed class CacheMetricsSnapshot
    {
        public long Hits { get; set; }
        public long Misses { get; set; }
        public long Sets { get; set; }
        public long Removals { get; set; }
        public long Expirations { get; set; }
        public long TotalReads { get; set; }
        public double HitRatioPercent { get; set; }
        public int TrackedEntryCount { get; set; }
        public double? AvgCachedMs { get; set; }
        public double? AvgUncachedMs { get; set; }
        public long OutputHits { get; set; }
        public long OutputMisses { get; set; }
        public double OutputHitRatioPercent { get; set; }
        public double? OutputAvgCachedMs { get; set; }
        public double? OutputAvgUncachedMs { get; set; }
    }

    public sealed class CacheEntrySnapshot
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string CacheKind { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string TypeName { get; set; }
        public string Size { get; set; }
        public DateTimeOffset? CreatedUtc { get; set; }
        public DateTimeOffset? ExpiresUtc { get; set; }
        public string Ttl { get; set; }
        public long HitCount { get; set; }
        public long Misses { get; set; }
        public string MissCount { get; set; }
        public double HitRatioPercent { get; set; }
        public double? AvgCachedMs { get; set; }
        public double? AvgUncachedMs { get; set; }
        public double? ImprovementPercent { get; set; }
        public DateTimeOffset? LastAccessUtc { get; set; }
        public string SlidingSeconds { get; set; }
    }

    public sealed class CacheEntryQueryResult
    {
        public IReadOnlyList<CacheEntrySnapshot> Entries { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public IReadOnlyList<string> Categories { get; set; }
    }
}
