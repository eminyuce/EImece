using EImece.Domain.Entities;

namespace EImece.Domain.Services.IServices
{
    public interface ITemplateService : IBaseEntityService<Template>
    {
        Template GetTemplate(int id);
    }
}