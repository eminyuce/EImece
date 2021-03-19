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
            List<Template> result = null;
            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = TemplateRepository.GetAllActiveTemplates();
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheLongSeconds);
            }
            return result;
        }

        public override Template GetSingle(int id)
        {
            return GetTemplate(id);
        }

        public Template GetTemplate(int id)
        {
            if (id == 0)
                return new Template();
            List<Template> resultList = null;

                resultList = GetAllActiveTemplates();
            var result = resultList.FirstOrDefault(r => r.Id == id);
            if (result == null)
            {
                Logger.Error("GetTemplate is null for id" + id);
            }
            return result;
        }
    }
}