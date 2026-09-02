using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.DTOs
{
    public class CustomerRegistrationDto
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FirstName))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.LastName2))]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.PhoneNumber))]
        public string PhoneNumber { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsPermissionGrantedDescription))]
        public bool IsPermissionGranted { get; set; }
    }
}
