using EImece.Domain.Entities;
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
    }
}