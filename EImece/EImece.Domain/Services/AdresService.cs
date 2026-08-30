using EImece.Domain.Caching;
using EImece.Domain.Models.FrontModels.Il_Ilce_Mahalle;
using Newtonsoft.Json;
using System;
using System.IO;

namespace EImece.Domain.Services
{
    public class AdresService
    {
        private readonly IEimeceCacheProvider MemoryCacheProvider;

        public AdresService(IEimeceCacheProvider memoryCacheProvider)
        {
            MemoryCacheProvider = memoryCacheProvider ?? throw new ArgumentNullException(nameof(memoryCacheProvider));
        }

        public TurkiyeAdres GetTurkiyeAdres()
        {
            var cacheKey = String.Format("GetTurkiyeAdres");
            // Single-flight: the (file-backed) address tree is parsed once even under concurrent
            // misses instead of every caller deserializing the JSON files in parallel.
            return MemoryCacheProvider.GetOrAdd(cacheKey, BuildTurkiyeAdres, AppConfig.CacheVeryLongSeconds);
        }

        private TurkiyeAdres BuildTurkiyeAdres()
        {
            return new TurkiyeAdres
            {
                IlRoot = GetIlRoot(),
                IlceRoot = GetIlceRoot()
            };
        }

        public IlceRoot GetIlceRoot()
        {
            return JsonConvert.DeserializeObject<IlceRoot>(read(@"App_Data\il-ilce-mahalle\ilceler.json"));
        }

        public IlRoot GetIlRoot()
        {
            return JsonConvert.DeserializeObject<IlRoot>(read(@"App_Data\il-ilce-mahalle\iller.json"));
        }

        private static string read(string relativePath)
        {
            var cleanPath = relativePath.TrimStart('~', '/', '\\');
            var dataDir = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            string resolvedPath = null;

            if (!string.IsNullOrEmpty(dataDir))
            {
                // if cleanPath starts with "App_Data\", remove it when checking against dataDir
                var subPath = cleanPath.StartsWith("App_Data\\", StringComparison.OrdinalIgnoreCase) || cleanPath.StartsWith("App_Data/", StringComparison.OrdinalIgnoreCase)
                    ? cleanPath.Substring(9)
                    : cleanPath;
                var candidate = Path.Combine(dataDir, subPath);
                if (File.Exists(candidate))
                {
                    resolvedPath = candidate;
                }
            }

            if (resolvedPath == null)
            {
                var candidate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleanPath);
                if (File.Exists(candidate))
                {
                    resolvedPath = candidate;
                }
            }

            if (resolvedPath != null && File.Exists(resolvedPath))
            {
                return File.ReadAllText(resolvedPath);
            }

            return string.Empty;
        }
    }
}