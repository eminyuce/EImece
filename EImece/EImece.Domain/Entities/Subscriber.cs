using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class Subscriber : BaseEntity
    {
        [Required]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        [EmailAddress]
        public string Email { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Note))]
        public string Note { get; set; }
    }
}