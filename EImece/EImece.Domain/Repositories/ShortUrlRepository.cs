using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Linq;

namespace EImece.Domain.Repositories
{
    public class ShortUrlRepository : BaseEntityRepository<ShortUrl>, IShortUrlRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ShortUrlRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public static int ShortUrlKeyLength
        {
            get
            {
                return AppConfig.GetConfigInt("ShortUrlKeyLength", 5);
            }
        }

        public ShortUrl GetShortUrlByUrl(string url)
        {
            return GetAll().FirstOrDefault(r => r.Url.Equals(url, StringComparison.InvariantCultureIgnoreCase));
        }

        public ShortUrl GetShortUrlByKey(string key)
        {
            return GetAll().FirstOrDefault(r => r.UrlKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }

        public ShortUrl GenerateShortUrl(string url, string email, string group)
        {
            Uri safeUri;
            if (!SecurityHelper.IsSafeHttpRedirectUrl(url, out safeUri))
            {
                throw new ArgumentException("Only absolute http or https URLs are allowed.", nameof(url));
            }

            var newKey = Guid.NewGuid().ToString("N").Substring(0, ShortUrlKeyLength).ToLower();
            var item = new ShortUrl();
            item.Url = safeUri.ToString();
            item.UpdatedDate = DateTime.Now;
            item.CreatedDate = DateTime.Now;
            item.UrlKey = newKey;
            item.IsActive = false;
            item.Lang = AppConfig.MainLanguage;
            item.Name = "";
            item.Position = 0;
            item.RequestCount = 0;
            SaveOrEdit(item);
            return item;
        }
    }
}