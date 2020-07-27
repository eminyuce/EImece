using EImece.Domain.Models.Enums;
using Resources;
using System;
using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string ZipCode { get; set; }

        public string Street { get; set; }
        public string District { get; set; }

        public Address()
        {
        }

        public Address(AddressType addressType)
        {
            this.AddressType = (int)addressType;
        }
    }
}