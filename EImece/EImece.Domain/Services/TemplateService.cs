using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    /// <summary>
    /// Template reads for Admin / product-spec editing always hit the database.
    /// Cached active-template lists are for storefront-style bulk reads only.
    /// </summary>
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

        /// <summary>
        /// Storefront-oriented: cached list of active templates.
        /// Do not use from Admin screens that need live data.
        /// </summary>
        public List<Template> GetAllActiveTemplates()
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

        public override Template GetSingle(int id)
        {
            return GetTemplate(id);
        }

        /// <summary>
        /// Always loads from the database (Admin-safe). Does not use the active-template cache.
        /// </summary>
        public Template GetTemplate(int id)
        {
            if (id == 0)
            {
                return new Template();
            }

            var result = TemplateRepository.GetSingle(id);
            if (result == null)
            {
                Logger.LogError("GetTemplate is null for id" + id);
                return null;
            }

            if (!result.IsActive)
            {
                Logger.LogWarning("GetTemplate found inactive template id" + id);
                return null;
            }

            return result;
        }

        /// <summary>
        /// Always loads from the database (Admin-safe). Does not use the active-template cache.
        /// </summary>
        public async Task<Template> GetTemplateAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == 0)
            {
                return new Template();
            }

            var result = await TemplateRepository.GetSingleAsync(id).ConfigureAwait(false);
            if (result == null)
            {
                Logger.LogError("GetTemplateAsync is null for id" + id);
                return null;
            }

            if (!result.IsActive)
            {
                Logger.LogWarning("GetTemplateAsync found inactive template id" + id);
                return null;
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