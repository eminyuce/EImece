using Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Customer : BaseEntity
    {
        //      [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        //   [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.LastName))]
        [NotMapped]
        public string Surname { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string GsmNumber { get; set; }

        //     [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsEmailErrorMessage))]
        //     [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        [EmailAddress]
        [NotMapped]
        public string Email { get; set; }

        public string IdentityNumber { get; set; }

        //    [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string Description { get; set; }

        public string Ip { get; set; }

        //      [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string City { get; set; }

        //      [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string Country { get; set; }

        //    [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string ZipCode { get; set; }

        [NotMapped]
        public bool IsSameAsShippingAddress { get; set; }

        public String UserId { get; set; }
        public bool IsPermissionGranted { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.BirthDate))]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM.dd.yyyy}")]
        public DateTime BirthDate { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Gender))]
        public int Gender { get; set; }

        public string Street { get; set; }
        public string District { get; set; }

        [NotMapped]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.AnswerSecurityQuestion))]
        public String Captcha { get; set; }

        public String FullName
        {
            get
            {
                return String.Format("{0} {1}", Name, Surname);
            }
        }

        public bool isValid()
        {
            return !(string.IsNullOrEmpty(Name)
                            && string.IsNullOrEmpty(Surname)
                            && string.IsNullOrEmpty(GsmNumber)
                            && string.IsNullOrEmpty(Email)
                            && string.IsNullOrEmpty(Description)
                            && string.IsNullOrEmpty(City)
                            && string.IsNullOrEmpty(Country)
                            && string.IsNullOrEmpty(ZipCode));
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Name)
                            || string.IsNullOrEmpty(Surname)
                            || string.IsNullOrEmpty(GsmNumber)
                            || string.IsNullOrEmpty(Email)
                            || string.IsNullOrEmpty(District)
                            || string.IsNullOrEmpty(Description)
                             || string.IsNullOrEmpty(City)
                             || string.IsNullOrEmpty(Street)
                             || string.IsNullOrEmpty(Country)
                             || string.IsNullOrEmpty(ZipCode);
        }
    }
}