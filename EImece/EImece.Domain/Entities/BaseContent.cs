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
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Description))]
        public string Description { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageState))]
        public Boolean ImageState { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MetaKeywords))]
        public string MetaKeywords { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MainImageId))]
        [ForeignKey("MainImage")]
        public int? MainImageId { get; set; }

        public virtual FileStorage MainImage { get; set; }

        [NotMapped]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageHeight))]
        public int ImageHeight { get; set; }

        [NotMapped]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageWidth))]
        public int ImageWidth { get; set; }

        public string UpdateUserId { get; set; }
        public string AddUserId { get; set; }
    }
}