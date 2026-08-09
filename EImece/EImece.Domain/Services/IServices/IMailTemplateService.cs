using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IMailTemplateService : IBaseEntityService<MailTemplate>
    {
        MailTemplate GetMailTemplateByName(string templatename);

        Task<MailTemplate> GetMailTemplateByNameAsync(string templatename);

        OrderConfirmationEmailRazorTemplate GenerateOrderConfirmationEmailRazorTemplate(int orderId);

        Task<OrderConfirmationEmailRazorTemplate> GenerateOrderConfirmationEmailRazorTemplateAsync(int orderId);

        CompanyGotNewOrderEmailRazorTemplate GenerateCompanyGotNewOrderEmailRazorTemplate(int orderId);

        Task<CompanyGotNewOrderEmailRazorTemplate> GenerateCompanyGotNewOrderEmailRazorTemplateAsync(int orderId);

        List<MailTemplate> GetAllMailTemplatesWithCache();

        Task<List<MailTemplate>> GetAllMailTemplatesWithCacheAsync();
    }
}