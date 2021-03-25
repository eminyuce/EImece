using Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class ProductComment : BaseEntity
    {
        public int ProductId { get; set; }
        public string UserId { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Review))]
        public string Review { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MandatoryField))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ReviewSubject))]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(AdminResource.Rating))]
        public int Rating { get; set; }

        [NotMapped]
        public string SeoUrl { get; set; }
    }
}