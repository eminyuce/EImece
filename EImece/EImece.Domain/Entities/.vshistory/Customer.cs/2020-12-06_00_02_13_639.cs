using EImece.Domain.Helpers;
using Newtonsoft.Json;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.LastName))]
        public string Surname { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.PhoneNumber))]
        public string GsmNumber { get; set; }

        //     [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsEmailErrorMessage))]
        //     [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]

        [NotMapped]
        [EmailAddress(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Email))]
        public string Email { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.IdentityNumber))]
        public string IdentityNumber { get; set; }

        public string Ip { get; set; }

        [NotMapped]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsSameAsShippingAddress))]
        public bool IsSameAsShippingAddress { get; set; }

        public string UserId { get; set; }
 
        public bool IsPermissionGranted { get; set; }

        //   [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Gender))]
        public int Gender { get; set; }

      //  [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Street))]
        public string Street { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Town))]
        public string Town { get; set; }
        //  [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.District))]
        public string District { get; set; }

      //  [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.City))]
        public string City { get; set; }

     //   [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Country))]
        public string Country { get; set; }

     //   [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ZipCode))]
        public string ZipCode { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.CustomerOpenAddress))]
        public string Description { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Company))]
        public string Company { get; set; }

        [NotMapped]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.AnswerSecurityQuestion))]
        [JsonIgnore]
        public String Captcha { get; set; }

        [NotMapped]
        public List<Order> Orders { get; set; }
        [NotMapped]
        public String FullName
        {
            get
            {
                return String.Format("{0} {1}", Name.ToStr(), Surname.ToStr());
            }
        }
        [NotMapped]
        public String Address
        {
            get
            {
                return string.Format("{0} {6} {1} {6} {2} {6} {3} {6} {4} {6} {5} {6}",
                Street.ToStr(),
                District.ToStr(),
                Town.ToStr(),
                City.ToStr(),
                Country.ToStr(),
                Description.ToStr(),
                Environment.NewLine);
            }
        }
        [NotMapped]
        public String RegistrationAddress
        {
            get
            {
                return string.Format("{0} {1} {2} {3} {4} {5} {6}",
               District.ToStr(),
               Street.ToStr(),
               Town.ToStr(),
               ZipCode.ToStr(),
               Description.ToStr(),
               City.ToStr(),
               Country.ToStr());

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
                            && string.IsNullOrEmpty(Town)
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
                             || string.IsNullOrEmpty(Town)
                             || string.IsNullOrEmpty(Street)
                             || string.IsNullOrEmpty(Country)
                             || string.IsNullOrEmpty(ZipCode);
        }
    }
}