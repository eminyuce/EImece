using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class SettingLayoutViewModel
    {
        public Setting WebSiteCompanyPhoneAndLocation { get; set; }
        public Setting WebSiteCompanyEmailAddress { get; set; }     
        public Setting WebSiteLogo { get; set; }  
        public Setting CompanyName { get; set; }

        public bool isMobilePage { get; set; }
    }
}
