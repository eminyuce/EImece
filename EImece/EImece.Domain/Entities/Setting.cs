using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class Setting : BaseEntity
    {
        public string Description { get; set; }
        public string SettingKey { get; set; }
        [AllowHtml]
        public string SettingValue { get; set; }
    }
}
