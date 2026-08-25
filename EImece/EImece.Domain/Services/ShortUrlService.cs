using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class ShortUrlService : BaseEntityService<ShortUrl>, IShortUrlService
    {
        private readonly IShortUrlRepository _shortUrlRepository;

        public ShortUrlService(IShortUrlRepository repository) : base(repository)
        {
            _shortUrlRepository = repository;
        }

        public ShortUrl GetShortUrlByUrl(string url) => _shortUrlRepository.GetShortUrlByUrl(url);
        public ShortUrl GetShortUrlByKey(string key) => _shortUrlRepository.GetShortUrlByKey(key);
        public ShortUrl GenerateShortUrl(string url, string email, string group) => _shortUrlRepository.GenerateShortUrl(url, email, group);

        public async Task<ShortUrl> GetShortUrlByUrlAsync(string url) => await Task.FromResult(GetShortUrlByUrl(url)).ConfigureAwait(false);
        public async Task<ShortUrl> GetShortUrlByKeyAsync(string key) => await Task.FromResult(GetShortUrlByKey(key)).ConfigureAwait(false);
    }
}
