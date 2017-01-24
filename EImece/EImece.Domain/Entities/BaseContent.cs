using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int? MainImageId { get; set; }

        public virtual FileStorage MainImage { get; set; }

        [NotMapped]
        public int ImageHeight { get; set; }
        [NotMapped]
        public int ImageWidth { get; set; }

         
    }
}
