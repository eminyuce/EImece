using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class TemplateService : BaseEntityService<Template>, ITemplateService
    {
        public ITemplateRepository TemplateRepository { get; set; }

        public TemplateService(
            ITemplateRepository repository,
            IEimeceCacheProvider dataCachingProvider,
            ILogger<TemplateService> logger)
            : base(repository, dataCachingProvider, logger)
        {
            TemplateRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Template> GetAllActiveTemplates()
        {
            var list = LoadAllActiveTemplates();
            return list ?? new List<Template>();
        }

        private async Task<List<Template>> GetAllActiveTemplatesAsync()
        {
            var list = await LoadAllActiveTemplatesAsync().ConfigureAwait(false);
            return list ?? new List<Template>();
        }

        private List<Template> LoadAllActiveTemplates()
        {
            if (DataCachingProvider == null)
            {
                return TemplateRepository.GetAllActiveTemplates() ?? new List<Template>();
            }

            var cacheKey = Constants.GetAllActiveTemplatesCacheKey;
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => TemplateRepository.GetAllActiveTemplates() ?? new List<Template>(),
                AppConfig.CacheLongSeconds) ?? new List<Template>();
        }

        private async Task<List<Template>> LoadAllActiveTemplatesAsync()
        {
            if (DataCachingProvider == null)
            {
                return await TemplateRepository.GetAllActiveTemplatesAsync(CancellationToken.None).ConfigureAwait(false)
                    ?? new List<Template>();
            }

            var cacheKey = Constants.GetAllActiveTemplatesCacheKey + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                async () => await TemplateRepository.GetAllActiveTemplatesAsync(CancellationToken.None).ConfigureAwait(false)
                    ?? new List<Template>(),
                AppConfig.CacheLongSeconds).ConfigureAwait(false) ?? new List<Template>();
        }

        public override Template GetSingle(int id)
        {
            return GetTemplate(id);
        }

        public Template GetTemplate(int id)
        {
            if (id == 0)
                return new Template();
            List<Template> resultList = GetAllActiveTemplates();
            var result = resultList.FirstOrDefault(r => r.Id == id);
            if (result == null)
            {
                // Cache can be stale after seed/admin template inserts; load directly from DB.
                result = TemplateRepository.GetSingle(id);
                if (result == null)
                {
                    Logger.LogError("GetTemplate is null for id" + id);
                }
                else if (!result.IsActive)
                {
                    Logger.LogWarning("GetTemplate found inactive template id" + id);
                    result = null;
                }
                else
                {
                    Logger.LogWarning("GetTemplate cache miss for id" + id + "; loaded from database.");
                    if (DataCachingProvider != null)
                    {
                        DataCachingProvider.Clear(Constants.GetAllActiveTemplatesCacheKey);
                    }
                }
            }
            return result;
        }

        public async Task<Template> GetTemplateAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == 0)
                return new Template();

            List<Template> resultList = await GetAllActiveTemplatesAsync().ConfigureAwait(false);
            var result = resultList.FirstOrDefault(r => r.Id == id);
            if (result == null)
            {
                result = await TemplateRepository.GetSingleAsync(id).ConfigureAwait(false);
                if (result == null)
                {
                    Logger.LogError("GetTemplateAsync is null for id" + id);
                }
                else if (!result.IsActive)
                {
                    Logger.LogWarning("GetTemplateAsync found inactive template id" + id);
                    result = null;
                }
                else
                {
                    Logger.LogWarning("GetTemplateAsync cache miss for id" + id + "; loaded from database.");
                    if (DataCachingProvider != null)
                    {
                        DataCachingProvider.Clear(Constants.GetAllActiveTemplatesCacheKey);
                        DataCachingProvider.Clear(Constants.GetAllActiveTemplatesCacheKey + AsyncCacheKeySuffix);
                    }
                }
            }
            return result;
        }

        public string GetTemplateXml(int id)
        {
            return TemplateRepository.GetTemplateXml(id);
        }

        public async Task<string> GetTemplateXmlAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await TemplateRepository.GetTemplateXmlAsync(id, cancellationToken).ConfigureAwait(false);
        }
    }
}
