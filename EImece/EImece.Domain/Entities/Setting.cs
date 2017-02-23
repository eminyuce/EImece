using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Resources;

namespace EImece.Domain.Entities
{
    public class Setting : BaseEntity
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Description))]
        public string Description { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.SettingKey))]
        public string SettingKey { get; set; }
        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.SettingValue))]
        public string SettingValue { get; set; }
    }
}
