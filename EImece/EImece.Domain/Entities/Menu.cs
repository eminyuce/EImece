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
        public int? ParentId { get; set; }
        public Boolean MainPage { get; set; }
        [NotMapped]
        public string Modul { get; set; }
        private string _action = String.Empty;
        public string Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (!String.IsNullOrEmpty(Modul))
                {
                    if (Modul.Equals("products",StringComparison.InvariantCultureIgnoreCase))
                    {
                        _action = "index";
                    }
                }
                else
                {
                    _action = value;
                }
           
            }
        }
        private string _controller = String.Empty;
        public string Controller
        {
            get
            {
                return _controller;
            }
            set
            {
                if (!String.IsNullOrEmpty(Modul))
                {
                    if (Modul.Equals("products", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _controller = "products";
                    }
                }
                else
                {
                    _controller = value;
                }

            }
        }
        public Boolean Static { get; set; }
        public string Link { get; set; }
        public Boolean LinkIsActive { get; set; }
        [NotMapped]
        public List<Menu> Childrens { get; set; }
    }
}
