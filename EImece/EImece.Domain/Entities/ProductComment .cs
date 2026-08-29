using Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class ProductComment : BaseEntity
    {
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        public string UserId { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Review))]
        public string Review { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ReviewSubject))]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Range(1, 5, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Rating))]
        public int Rating { get; set; }

        // Needed for Admin panel — admin product-comment moderation links back to the storefront SEO URL.
        [NotMapped]
        public string SeoUrl { get; set; }

        // Needed for Admin panel — all-comments grid sort/display without null Product navigation.
        [NotMapped]
        public string ProductName
        {
            get { return Product != null ? Product.Name : ""; }
        }
    }
}