using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Services
{
    public class TemplateService : BaseEntityService<Template>, ITemplateService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ITemplateRepository TemplateRepository { get; set; }

        public TemplateService(ITemplateRepository repository) : base(repository)
        {
            TemplateRepository = repository;
        }

        public List<Template> GetAllActiveTemplates()
        {
            var cacheKey = String.Format("GetAllActiveTemplates");
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => TemplateRepository.GetAllActiveTemplates(),
                AppConfig.CacheLongSeconds);
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
                    Logger.Error("GetTemplate is null for id" + id);
                }
                else if (!result.IsActive)
                {
                    Logger.Warn("GetTemplate found inactive template id" + id);
                    result = null;
                }
                else
                {
                    Logger.Warn("GetTemplate cache miss for id" + id + "; loaded from database.");
                    DataCachingProvider.Clear(String.Format("GetAllActiveTemplates"));
                }
            }
            return result;
        }
    }
}