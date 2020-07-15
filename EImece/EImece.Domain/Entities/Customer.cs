using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Customer : BaseEntity
    {
        // Entity annotions
        //[DataType(DataType.Text)]
        //[StringLength(100, ErrorMessage = "TestColumnName cannot be longer than 100 characters.")]
        //[Display(Name ="TestColumnName")]
        //[Required(ErrorMessage ="TestColumnName")]
        //[AllowHtml]

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.LastName))]
        public string Surname { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string GsmNumber { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsEmailErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime RegistrationDate { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string RegistrationAddress { get; set; }
        public string Ip { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string City { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string Country { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string ZipCode { get; set; }

        public bool IsSameAsShippingAddress { get; set; }

        public String FullName
        {
            get {
                return String.Format("{0} {1}", Name, Surname);
             }
        }


    public Boolean isValid()
        {
            return !(string.IsNullOrEmpty(Name)  
                && string.IsNullOrEmpty(Surname) && string.IsNullOrEmpty(GsmNumber)
                && string.IsNullOrEmpty(Email) 
                            && string.IsNullOrEmpty(RegistrationAddress) && string.IsNullOrEmpty(City)
                                            && string.IsNullOrEmpty(Country) && string.IsNullOrEmpty(ZipCode));
        }
    }
}
