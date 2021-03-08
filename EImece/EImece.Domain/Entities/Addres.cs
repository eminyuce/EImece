using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Address : BaseEntity
    {
        // Entity annotions
        //[DataType(DataType.Text)]
        //[StringLength(100, ErrorMessage = "TestColumnName cannot be longer than 100 characters.")]
        //[Display(Name ="TestColumnName")]
        //[Required(ErrorMessage ="TestColumnName")]
        //[AllowHtml]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string Description { get; set; }

        public int AddressType { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string City { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string Country { get; set; }

        public string ZipCode { get; set; }

        public string Street { get; set; }
        public string District { get; set; }

        [NotMapped]
        public String AddressInfo
        {
            get
            {
                return string.Format("{0} {1} {2} {3} {4} {5}",
               District.ToStr(),
               Street.ToStr(),
               ZipCode.ToStr(),
               Description.ToStr(),
               City.ToStr(),
               Country.ToStr());
            }
        }

        public Address()
        {
        }

        public Address(AddressType addressType)
        {
            this.AddressType = (int)addressType;
        }
    }
}