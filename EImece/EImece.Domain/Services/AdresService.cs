using EImece.Domain.Caching;
using EImece.Domain.Models.FrontModels.Il_Ilce_Mahalle;
using Newtonsoft.Json;
using Ninject;
using System;
using System.Web;

namespace EImece.Domain.Services
{
    public class AdresService
    {
        public AdresService()
        {
        }

        private ICacheProvider _memoryCacheProvider { get; set; }

        [Inject]
        public ICacheProvider MemoryCacheProvider
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
            TurkiyeAdres turkiyeAdres = null;
            if (!MemoryCacheProvider.Get(cacheKey, out turkiyeAdres))
            {
                turkiyeAdres = GetTurkiyeAdres(cacheKey);
            }

            if (turkiyeAdres == null)
            {
                turkiyeAdres = GetTurkiyeAdres(cacheKey);
            }

            return turkiyeAdres;
        }

        private TurkiyeAdres GetTurkiyeAdres(string cacheKey)
        {
            TurkiyeAdres turkiyeAdres = new TurkiyeAdres();
            turkiyeAdres.IlRoot = GetIlRoot();
            turkiyeAdres.IlceRoot = GetIlceRoot();
            MemoryCacheProvider.Set(cacheKey, turkiyeAdres, AppConfig.CacheVeryLongSeconds);
            return turkiyeAdres;
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
            string filePath2 = HttpContext.Current.Server.MapPath(filePath);
            string result = System.IO.File.ReadAllText(filePath2);
            return result;
        }
    }
}