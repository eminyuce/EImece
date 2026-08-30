using System.Web.Mvc;

namespace EImece.Models.Admin
{
    public class BaseContentDescriptionMetadata
    {
        [AllowHtml]
        public string Description { get; set; }
    }

    public class ProductHtmlMetadata : BaseContentDescriptionMetadata
    {
        [AllowHtml]
        public string ShortDescription { get; set; }
    }

    public class ProductCategoryHtmlMetadata : BaseContentDescriptionMetadata
    {
        [AllowHtml]
        public string ShortDescription { get; set; }
    }

    public class StoryHtmlMetadata : BaseContentDescriptionMetadata
    {
        [AllowHtml]
        public string ShortDescription { get; set; }
    }

    public class FaqHtmlMetadata
    {
        [AllowHtml]
        public string Question { get; set; }

        [AllowHtml]
        public string Answer { get; set; }
    }

    public class MailTemplateHtmlMetadata
    {
        [AllowHtml]
        public string Body { get; set; }
    }

    public class SettingHtmlMetadata
    {
        [AllowHtml]
        public string Description { get; set; }

        [AllowHtml]
        public string SettingValue { get; set; }
    }

    public class CustomerHtmlMetadata
    {
        [AllowHtml]
        public string Description { get; set; }
    }

    public class SettingModelHtmlMetadata
    {
        [AllowHtml]
        public string SiteIndexMetaDescription { get; set; }

        [AllowHtml]
        public string FooterDescription { get; set; }

        [AllowHtml]
        public string FooterEmailListDescription { get; set; }

        [AllowHtml]
        public string FooterHtmlDescription { get; set; }

        [AllowHtml]
        public string CargoDescription { get; set; }

        [AllowHtml]
        public string CompanyAddress { get; set; }
    }

    public class SystemSettingModelHtmlMetadata
    {
        [AllowHtml]
        public string GoogleAnalyticsTrackingScript { get; set; }

        [AllowHtml]
        public string WhatsAppCommunicationScript { get; set; }

        [AllowHtml]
        public string GoogleMapScript { get; set; }

        [AllowHtml]
        public string Zopim { get; set; }

        [AllowHtml]
        public string PaymentDetailHtml { get; set; }

        [AllowHtml]
        public string UnderConstructionHtml { get; set; }

        [AllowHtml]
        public string ProductPriceFilterSetting { get; set; }
    }
}
