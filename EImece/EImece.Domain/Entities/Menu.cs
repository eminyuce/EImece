using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class Menu : BaseContent
    {
        public int ParentId { get; set; }
        public Boolean MainPage { get; set; }
        public string MenuLink   { get; set; }
        public Boolean Static { get; set; }
        public string Link { get; set; }
       
        public string PageTheme { get; set; }
        public Boolean LinkIsActive { get; set; }
        [NotMapped]
        public List<Menu> Childrens { get; set; }
    }
}
