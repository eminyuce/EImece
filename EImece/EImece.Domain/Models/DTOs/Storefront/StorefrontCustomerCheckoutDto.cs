using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Storefront-only checkout form model. Holds transient UI fields that were previously
    /// NotMapped on the Customer entity (Captcha, IsSameAsShippingAddress) and are not persisted.
    /// </summary>
    public class StorefrontCustomerCheckoutDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string GsmNumber { get; set; }
        public string City { get; set; }
        public string Town { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string Description { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsSameAsShippingAddress))]
        public bool IsSameAsShippingAddress { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AnswerSecurityQuestion))]
        public string Captcha { get; set; }

        public string FullName => string.Format("{0} {1}", Name.ToStr(), Surname.ToStr()).Trim();

        public static StorefrontCustomerCheckoutDto FromEntity(Customer c)
        {
            if (c == null) return null;
            return new StorefrontCustomerCheckoutDto
            {
                Id = c.Id,
                Name = c.Name,
                Surname = c.Surname,
                Email = c.Email,
                GsmNumber = c.GsmNumber,
                City = c.City,
                Town = c.Town,
                District = c.District,
                Street = c.Street,
                Country = c.Country,
                ZipCode = c.ZipCode,
                Description = c.Description
            };
        }

        public Customer ToEntity()
        {
            return new Customer
            {
                Id = Id,
                Name = Name,
                Surname = Surname,
                Email = Email,
                GsmNumber = GsmNumber,
                City = City,
                Town = Town,
                District = District,
                Street = Street,
                Country = Country,
                ZipCode = ZipCode,
                Description = Description
            };
        }
    }
}
