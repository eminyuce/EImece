using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class FooterViewModel
    {
        public List<StorefrontMenuNavigationDto> Menus { get; set; }
        public List<StorefrontCategoryDto> ProductCategories { get; set; }
        public SettingValueDto FooterLogo { get; set; }
        public SettingValueDto CompanyName { get; set; }
        public SettingValueDto CompanyAddress { get; set; }
        public SettingValueDto FooterDescription { get; set; }
        public SettingValueDto FooterHtmlDescription { get; set; }

        public SettingValueDto FooterEmailListDescription { get; set; }

        public FooterViewModel()
        {
            Menus = new List<StorefrontMenuNavigationDto>();
            ProductCategories = new List<StorefrontCategoryDto>();
        }
    }
}