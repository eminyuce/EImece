using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public  class ContactUsFormViewModel
    {
        public String Name { get; set; }
        public String Email { get; set; }
        public String CompanyName { get; set; }
        public String Phone { get; set; }
        public String Address { get; set; }
        public String Message { get; set; }
    }
}
