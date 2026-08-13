namespace EImece.Domain.Models.AdminModels
{
    public class MailTemplateTestRenderResult
    {
        public bool Success { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string ErrorMessage { get; set; }
    }
}
