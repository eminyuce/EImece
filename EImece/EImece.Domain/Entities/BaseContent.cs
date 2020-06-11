using Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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


        public string MetaKeywords { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainImageId))]
        [ForeignKey("MainImage")]
        public int? MainImageId { get; set; }

        public virtual FileStorage MainImage { get; set; }

        [NotMapped]
        public int ImageHeight { get; set; }
        [NotMapped]
        public int ImageWidth { get; set; }

        public string UpdateUserId { get; set; }
        public string AddUserId { get; set; }


    }
}
