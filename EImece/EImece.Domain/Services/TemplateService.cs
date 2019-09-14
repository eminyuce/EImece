using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;
using NLog;

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
                MemoryCacheProvider.Set(cacheKey, result, ApplicationConfigs.CacheLongSeconds);
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
            var cacheKey = String.Format("Template-{0}", id);
            Template result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = GetSingle(id);
                MemoryCacheProvider.Set(cacheKey, result, ApplicationConfigs.CacheMediumSeconds);
            }
            return result;
        }
    }
}
