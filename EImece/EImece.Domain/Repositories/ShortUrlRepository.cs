using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.Models.Enums;
using GenericRepository.EntityFramework.Enums;
using System.Linq.Expressions;
using NLog;

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
                return ApplicationConfigs.GetConfigInt("ShortUrlKeyLength", 5);
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
            var newKey = Guid.NewGuid().ToString("N").Substring(0, ShortUrlKeyLength).ToLower();
            var item = new ShortUrl();
            item.Url = url;
            item.UpdatedDate = DateTime.Now;
            item.CreatedDate = DateTime.Now;
            item.UrlKey = newKey;
            item.IsActive = false;
            item.Lang = ApplicationConfigs.MainLanguage;
            item.Name = "";
            item.Position = 0;
            item.RequestCount = 0;
            SaveOrEdit(item);
            return item;

        }
    }
}
