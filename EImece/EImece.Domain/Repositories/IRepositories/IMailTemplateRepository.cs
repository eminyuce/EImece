using EImece.Domain.Entities;
using System;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IMailTemplateRepository : IBaseEntityRepository<MailTemplate>
    {
        MailTemplate GetMailTemplateByName(string templatename);
    }
}