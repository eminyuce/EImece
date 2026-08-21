using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class FooterViewModel
    {
        public List<StorefrontMenuDto> Menus { get; set; }
        public List<StorefrontCategoryDto> ProductCategories { get; set; }
        public SettingDto FooterLogo { get; set; }
        public SettingDto CompanyName { get; set; }
        public SettingDto CompanyAddress { get; set; }
        public SettingDto FooterDescription { get; set; }
        public SettingDto FooterHtmlDescription { get; set; }

        public SettingDto FooterEmailListDescription { get; set; }

        public FooterViewModel()
        {
            Menus = new List<StorefrontMenuDto>();
            ProductCategories = new List<StorefrontCategoryDto>();
        }
    }
}