using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class MailTemplate : BaseEntity
    {
        public String Subject { get; set; }
        [AllowHtml]
        public String Body { get; set; }

    }
}
