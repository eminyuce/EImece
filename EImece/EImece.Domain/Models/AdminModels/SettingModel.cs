using Resources;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EImece.Domain.Models.AdminModels
{
    public class SettingModel
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.SiteIndexMetaTitle))]
        public string SiteIndexMetaTitle { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.SiteIndexMetaDescription))]
        public string SiteIndexMetaDescription { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.SiteIndexMetaKeywords))]
        public string SiteIndexMetaKeywords { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FooterDescription))]
        public string FooterDescription { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FooterEmailListDescription))]
        public string FooterEmailListDescription { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FooterHtmlDescription))]
        [AllowHtml]
        public string FooterHtmlDescription { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.CargoCompany))]
        public string CargoCompany { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.CargoPrice))]
        public int CargoPrice { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.CargoDescription))]
        [AllowHtml]
        public string CargoDescription { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.BasketMinTotalPriceForCargo))]
        public int BasketMinTotalPriceForCargo { get; set; }
    }
}