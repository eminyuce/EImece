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

        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.EmailRequired))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.PhoneNumber))]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IdentityNumber))]
        [StringLength(11, MinimumLength = 11, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string IdentityNumber { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Street))]
        public string Street { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Town))]
        public string Town { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.District))]
        public string District { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.City))]
        public string City { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Country))]
        public string Country { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ZipCode))]
        public string ZipCode { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsPermissionGrantedDescription))]
        public bool IsPermissionGranted { get; set; }
    }
}
