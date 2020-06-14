using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class Subscriber : BaseEntity
    {
        [Required]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Email))]
        public string Email { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Note))]
        public string Note { get; set; }
    }
}