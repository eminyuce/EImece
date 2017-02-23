using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Description))]
        public string Description { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ImageState))]
        public Boolean ImageState { get; set; }


        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainImageId))]
        public int? MainImageId { get; set; }

        public  FileStorage MainImage { get; set; }

        [NotMapped]
        public int ImageHeight { get; set; }
        [NotMapped]
        public int ImageWidth { get; set; }

         
    }
}
