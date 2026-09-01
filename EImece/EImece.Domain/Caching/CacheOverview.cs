using System;
using System.Globalization;

namespace EImece.Domain.Caching
{
    public enum CacheEffectivenessLevel
    {
        Effective = 0,
        Limited = 1,
        NotEffective = 2
    }

    public sealed class CacheLayerSnapshot
    {
        public string Kind { get; set; }
        public bool IsEnabled { get; set; }
        public long Hits { get; set; }
        public long Misses { get; set; }
        public long TotalReads { get; set; }
        public double HitRatioPercent { get; set; }
        public int ActiveEntries { get; set; }
        public double? AvgCachedMs { get; set; }
        public double? AvgUncachedMs { get; set; }
        public double? ImprovementPercent { get; set; }
        public double? SavedMs { get; set; }
        public bool HasTiming { get; set; }
        public long? EstimatedDatabaseOperationsAvoided { get; set; }
        public CacheEffectivenessLevel Effectiveness { get; set; }
    }

    public sealed class CacheOverviewSnapshot
    {
        public bool IsCacheActive { get; set; }
        public CacheLayerSnapshot Combined { get; set; }
        public CacheLayerSnapshot Page { get; set; }
        public CacheLayerSnapshot Data { get; set; }
        public string[] PageCacheProfiles { get; set; }
    }

    public static class CacheHealth
    {
        public const int MinReadsForHitRate = 8;

        public static CacheEffectivenessLevel Evaluate(bool isEnabled, long hits, long misses, double? improvementPercent)
        {
            if (!isEnabled)
            {
                return CacheEffectivenessLevel.NotEffective;
            }

            var total = hits + misses;
            if (total < MinReadsForHitRate)
            {
                return CacheEffectivenessLevel.Limited;
            }

            var ratio = hits / (double)total * 100d;
            if (ratio >= 60d)
            {
                if (improvementPercent.HasValue && improvementPercent.Value < 20d)
                {
                    return CacheEffectivenessLevel.Limited;
                }

                return CacheEffectivenessLevel.Effective;
            }

            if (ratio >= 20d)
            {
                return CacheEffectivenessLevel.Limited;
            }

            return CacheEffectivenessLevel.NotEffective;
        }

        public static double? ImprovementPercent(double? avgUncachedMs, double? avgCachedMs)
        {
            if (!avgUncachedMs.HasValue || !avgCachedMs.HasValue || avgUncachedMs.Value <= 0d)
            {
                return null;
            }

            if (avgUncachedMs.Value <= avgCachedMs.Value)
            {
                return 0d;
            }

            return Math.Round((avgUncachedMs.Value - avgCachedMs.Value) / avgUncachedMs.Value * 100d, 1, MidpointRounding.AwayFromZero);
        }

        public static double? SavedMs(long hits, double? avgUncachedMs, double? avgCachedMs)
        {
            if (hits <= 0 || !avgUncachedMs.HasValue || !avgCachedMs.HasValue)
            {
                return null;
            }

            var delta = avgUncachedMs.Value - avgCachedMs.Value;
            if (delta <= 0d)
            {
                return null;
            }

            return hits * delta;
        }

        public static double? AverageMs(long ticks, long samples)
        {
            if (samples <= 0 || ticks < 0)
            {
                return null;
            }

            var ms = ticks / (double)samples / TimeSpan.TicksPerMillisecond;
            if (ms > 0d && ms < 0.01d)
            {
                return Math.Round(ms, 3, MidpointRounding.AwayFromZero);
            }

            return Math.Round(ms, 2, MidpointRounding.AwayFromZero);
        }

        public static double HitRatioPercent(long hits, long misses)
        {
            var total = hits + misses;
            if (total <= 0)
            {
                return 0d;
            }

            return Math.Round((hits / (double)total) * 100d, 2, MidpointRounding.AwayFromZero);
        }

        public static double? WeightedAverage(double? firstMs, long firstSamples, double? secondMs, long secondSamples)
        {
            if (firstSamples <= 0 && secondSamples <= 0)
            {
                return null;
            }

            if (firstSamples <= 0)
            {
                return secondMs;
            }

            if (secondSamples <= 0)
            {
                return firstMs;
            }

            if (!firstMs.HasValue || !secondMs.HasValue)
            {
                return firstMs ?? secondMs;
            }

            return Math.Round(
                ((firstMs.Value * firstSamples) + (secondMs.Value * secondSamples)) / (firstSamples + secondSamples),
                2,
                MidpointRounding.AwayFromZero);
        }

        public static string FormatMilliseconds(double? ms)
        {
            if (!ms.HasValue)
            {
                return null;
            }

            if (ms.Value >= 1000d)
            {
                return (ms.Value / 1000d).ToString("N2", CultureInfo.CurrentCulture) + " s";
            }

            if (ms.Value > 0d && ms.Value < 1d)
            {
                return ms.Value.ToString("N3", CultureInfo.CurrentCulture) + " ms";
            }

            return ms.Value.ToString("N1", CultureInfo.CurrentCulture) + " ms";
        }

        public static string FormatSaved(double? savedMs)
        {
            if (!savedMs.HasValue || savedMs.Value <= 0d)
            {
                return null;
            }

            if (savedMs.Value >= 60000d)
            {
                return (savedMs.Value / 60000d).ToString("N1", CultureInfo.CurrentCulture) + " min";
            }

            if (savedMs.Value >= 1000d)
            {
                return (savedMs.Value / 1000d).ToString("N1", CultureInfo.CurrentCulture) + " s";
            }

            return savedMs.Value.ToString("N0", CultureInfo.CurrentCulture) + " ms";
        }

        public static string FormatImprovement(double? percent)
        {
            if (!percent.HasValue)
            {
                return null;
            }

            return percent.Value.ToString("N1", CultureInfo.CurrentCulture) + "% faster";
        }
    }
}
