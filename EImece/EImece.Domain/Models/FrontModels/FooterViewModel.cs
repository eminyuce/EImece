using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class FooterViewModel
    {
        public List<Menu> Menus { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
        public Setting FooterLogo { get; set; }
        public Setting CompanyName { get; set; }
        public Setting FooterDescription { get; set; }
        public Setting FooterEmailListDescription { get; set; }
    }
}
