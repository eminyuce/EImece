using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EImece.Domain.Repositories.IRepositories
{
    
    public interface IShortUrlRepository : IBaseEntityRepository<ShortUrl>, IDisposable
    {
        ShortUrl GetShortUrlByUrl(string url);
        ShortUrl GetShortUrlByKey(string key);
        ShortUrl GenerateShortUrl(string url, string email, string group);
    }
}
