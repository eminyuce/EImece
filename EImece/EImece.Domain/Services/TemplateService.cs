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
    }
}
