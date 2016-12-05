using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public abstract class BaseContent : BaseEntity
    {
        [AllowHtml]
        public string Description { get; set; }
        public Boolean ImageState { get; set; }
        public string ImageUrl { get; set; }
    }
}
