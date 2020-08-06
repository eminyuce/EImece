using EImece.Domain.Entities;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IShortUrlRepository : IBaseEntityRepository<ShortUrl>
    {
        ShortUrl GetShortUrlByUrl(string url);

        ShortUrl GetShortUrlByKey(string key);

        ShortUrl GenerateShortUrl(string url, string email, string group);
    }
}