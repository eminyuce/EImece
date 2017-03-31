using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class MailTemplate : BaseEntity
    {
        public String Subject { get; set; }
        public String Body { get; set; }
        public String MailFrom { get; set; }

    }
}
