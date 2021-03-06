using EImece.Domain.Helpers;
using Newtonsoft.Json;
using Resources;
using System;
using System.Collections.Generic;
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
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IdentityNumber))]
        public string IdentityNumber { get; set; }

        public string Ip { get; set; }

        [NotMapped]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsSameAsShippingAddress))]
        public bool IsSameAsShippingAddress { get; set; }

        public string UserId { get; set; }

        public bool IsPermissionGranted { get; set; }

        //   [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Gender))]
        public int Gender { get; set; }

        //  [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Street))]
        public string Street { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Town))]
        public string Town { get; set; }

        //  [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.District))]
        public string District { get; set; }

        //  [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.City))]
        public string City { get; set; }

        //   [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Country))]
        public string Country { get; set; }

        //   [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ZipCode))]
        public string ZipCode { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.CustomerOpenAddress))]
        public string Description { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Company))]
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

        public bool isValidCustomer()
        {
            return !string.IsNullOrEmpty(Name)
                            && !string.IsNullOrEmpty(Surname)
                            && !string.IsNullOrEmpty(GsmNumber)
                            && !string.IsNullOrEmpty(Email)
                            && !string.IsNullOrEmpty(City)
                            && !string.IsNullOrEmpty(Town)
                            && !string.IsNullOrEmpty(Country);
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Name)
                            || string.IsNullOrEmpty(Surname)
                            || string.IsNullOrEmpty(GsmNumber)
                            || string.IsNullOrEmpty(Email)
                            || string.IsNullOrEmpty(District)
                             || string.IsNullOrEmpty(City)
                             || string.IsNullOrEmpty(Town)
                             || string.IsNullOrEmpty(Street)
                             || string.IsNullOrEmpty(Country) ;
        }

        public override string ToString()
        {
            return $"{{{nameof(Surname)}={Surname}, {nameof(GsmNumber)}={GsmNumber}, {nameof(Email)}={Email}, {nameof(IdentityNumber)}={IdentityNumber}, {nameof(Ip)}={Ip}, {nameof(IsSameAsShippingAddress)}={IsSameAsShippingAddress.ToString()}, {nameof(UserId)}={UserId}, {nameof(IsPermissionGranted)}={IsPermissionGranted.ToString()}, {nameof(Gender)}={Gender.ToString()}, {nameof(Street)}={Street}, {nameof(Town)}={Town}, {nameof(District)}={District}, {nameof(City)}={City}, {nameof(Country)}={Country}, {nameof(ZipCode)}={ZipCode}, {nameof(Description)}={Description}, {nameof(Company)}={Company}, {nameof(Captcha)}={Captcha}, {nameof(Orders)}={Orders}, {nameof(FullName)}={FullName}, {nameof(Address)}={Address}, {nameof(RegistrationAddress)}={RegistrationAddress}, {nameof(Id)}={Id.ToString()}, {nameof(Name)}={Name}, {nameof(CreatedDate)}={CreatedDate.ToString()}, {nameof(UpdatedDate)}={UpdatedDate.ToString()}, {nameof(IsActive)}={IsActive.ToString()}, {nameof(Position)}={Position.ToString()}, {nameof(Lang)}={Lang.ToString()}}}";
        }
    }
}