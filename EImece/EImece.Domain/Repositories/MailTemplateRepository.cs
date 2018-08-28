using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using NLog;
using Ninject;
using EImece.Domain.ApiRepositories;
using EImece.Domain.Models.UrlShortenModels;

namespace EImece.Domain.Repositories
{
    public class MailTemplateRepository : BaseEntityRepository<MailTemplate>, IMailTemplateRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public BitlyRepository BitlyRepository { get; set; }

        public MailTemplateRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }

        public MailTemplate GetMailTemplateByName(string templatename)
        {
            var item = GetAll().FirstOrDefault(r => r.Name.Equals(templatename));
           

            return item;
        }
    }
}

