namespace EImece.Domain.Models.AdminModels
{
    public class MailTemplateDummyDataContext
    {
        public string BaseUrl { get; set; }

        public string CompanyName { get; set; }

        public string CompanyEmail { get; set; }

        public string CompanyAddress { get; set; }

        public string CompanyPhone { get; set; }

        public string LogoUrl { get; set; }

        public string RecipientEmail { get; set; }

        public static MailTemplateDummyDataContext CreateDefaults()
        {
            return new MailTemplateDummyDataContext
            {
                BaseUrl = "https://example.com",
                CompanyName = "Örnek Şirket",
                CompanyEmail = "info@example.com",
                CompanyAddress = "Örnek Mah. Test Cad. No:1 İstanbul",
                CompanyPhone = "+90 555 000 00 00",
                LogoUrl = "https://example.com/images/logo.jpg",
                RecipientEmail = "test@example.com"
            };
        }
    }
}
