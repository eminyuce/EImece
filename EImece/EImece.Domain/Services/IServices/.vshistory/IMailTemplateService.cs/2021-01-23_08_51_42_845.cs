using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;

namespace EImece.Domain.Services.IServices
{
    public interface IMailTemplateService : IBaseEntityService<MailTemplate>
    {
        MailTemplate GetMailTemplateByName(string templatename);

        OrderConfirmationEmailRazorTemplate GenerateOrderConfirmationEmailRazorTemplate(int orderId);

        CompanyGotNewOrderEmailRazorTemplate GenerateCompanyGotNewOrderEmailRazorTemplate(int orderId);
        List<MailTemplate> GetAllMailTemplatesWithCache();
    }
}