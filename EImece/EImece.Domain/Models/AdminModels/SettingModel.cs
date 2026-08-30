using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.AdminModels
{
    public class SettingModel
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.SiteIndexMetaTitle))]
        public string SiteIndexMetaTitle { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.SiteIndexMetaDescription))]
        public string SiteIndexMetaDescription { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.SiteIndexMetaKeywords))]
        public string SiteIndexMetaKeywords { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.FooterDescription))]
        public string FooterDescription { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.FooterEmailListDescription))]
        public string FooterEmailListDescription { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.FooterHtmlDescription))]
        public string FooterHtmlDescription { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.CargoCompany))]
        public string CargoCompany { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.CargoPrice))]
        public int CargoPrice { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.CargoDescription))]
        public string CargoDescription { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.BasketMinTotalPriceForCargo))]
        public int BasketMinTotalPriceForCargo { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.CompanyName))]
        public string CompanyName { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.WebSiteCompanyEmailAddress))]
        public string WebSiteCompanyEmailAddress { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.WebSiteCompanyPhoneAndLocation))]
        public string WebSiteCompanyPhoneAndLocation { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.CompanyAddress))]
        public string CompanyAddress { get; set; }
    }
}