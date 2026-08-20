using EImece.Domain.Caching;
using EImece.Domain.Models.FrontModels.Il_Ilce_Mahalle;
using Newtonsoft.Json;
using EImece.Domain.DependencyInjection;
using System;
using System.Web;

namespace EImece.Domain.Services
{
    public class AdresService
    {
        public AdresService()
        {
        }

        private IEimeceCacheProvider _memoryCacheProvider { get; set; }

        [Inject]
        public IEimeceCacheProvider MemoryCacheProvider
        {
            get
            {
                return _memoryCacheProvider;
            }
            set
            {
                _memoryCacheProvider = value;
            }
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
            return JsonConvert.DeserializeObject<IlceRoot>(read(@"~\App_Data\il-ilce-mahalle\ilceler.json"));
        }

        public IlRoot GetIlRoot()
        {
            return JsonConvert.DeserializeObject<IlRoot>(read(@"~\App_Data\il-ilce-mahalle\iller.json"));
        }

        private static string read(string filePath)
        {
            string resolvedPath = null;
            if (System.Web.Hosting.HostingEnvironment.IsHosted)
            {
                resolvedPath = System.Web.Hosting.HostingEnvironment.MapPath(filePath);
            }
            else if (HttpContext.Current != null && HttpContext.Current.Server != null)
            {
                resolvedPath = HttpContext.Current.Server.MapPath(filePath);
            }
            else
            {
                string cleanPath = filePath.TrimStart('~', '/', '\\');
                resolvedPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleanPath);
            }

            if (resolvedPath != null && System.IO.File.Exists(resolvedPath))
            {
                return System.IO.File.ReadAllText(resolvedPath);
            }

            return string.Empty;
        }
    }
}