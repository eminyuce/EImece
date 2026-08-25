using EImece.Domain.Entities;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IShortUrlService : IBaseEntityService<ShortUrl>
    {
        ShortUrl GetShortUrlByUrl(string url);
        ShortUrl GetShortUrlByKey(string key);
        ShortUrl GenerateShortUrl(string url, string email, string group);
        Task<ShortUrl> GetShortUrlByUrlAsync(string url);
        Task<ShortUrl> GetShortUrlByKeyAsync(string key);
    }
}
