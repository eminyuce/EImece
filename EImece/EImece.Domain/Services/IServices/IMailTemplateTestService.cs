using EImece.Domain.Models.AdminModels;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IMailTemplateTestService
    {
        Task<MailTemplateTestPreview> InspectAsync(SendMailTemplateTestRequest request, string defaultRecipientEmail);

        Task<MailTemplateTestRenderResult> PreviewAsync(SendMailTemplateTestRequest request);

        Task<MailTemplateTestSendResult> SendTestEmailAsync(SendMailTemplateTestRequest request);
    }
}
