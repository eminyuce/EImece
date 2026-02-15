using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class FooterViewModel
    {
        public List<Menu> Menus { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
        public Setting FooterLogo { get; set; }
        public Setting CompanyName { get; set; }
        public Setting CompanyAddress { get; set; }
        public Setting FooterDescription { get; set; }
        public Setting FooterHtmlDescription { get; set; }

        public Setting FooterEmailListDescription { get; set; }
        public List<MenuDto> MenusDto { get; set; }
        public List<ProductCategoryDto> ProductCategoriesDto { get; set; }
        public SettingDto FooterLogoDto { get; set; }
        public SettingDto CompanyNameDto { get; set; }
        public SettingDto CompanyAddressDto { get; set; }
        public SettingDto FooterDescriptionDto { get; set; }
        public SettingDto FooterHtmlDescriptionDto { get; set; }

        public SettingDto FooterEmailListDescriptionDto { get; set; }
    }
}