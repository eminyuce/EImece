using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.DTOs
{
    [Serializable]
    public class CustomerDto
    {
        // from BaseEntity
        public int Id { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Name))]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from Customer
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.LastName))]
        public string Surname { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.PhoneNumber))]
        public string GsmNumber { get; set; }

        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IdentityNumber))]
        public string IdentityNumber { get; set; }

        public string Ip { get; set; }
        public bool IsSameAsShippingAddress { get; set; }
        public string UserId { get; set; }
        public bool IsPermissionGranted { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Gender))]
        public int Gender { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Street))]
        public string Street { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Town))]
        public string Town { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.District))]
        public string District { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.City))]
        public string City { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Country))]
        public string Country { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ZipCode))]
        public string ZipCode { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.CustomerOpenAddress))]
        public string Description { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Company))]
        public string Company { get; set; }

        public int CustomerType { get; set; }
        public string Captcha { get; set; }
        public DateTime OrderLatestDate { get; set; }
        public DateTime? BirthDate { get; set; }

        private string _fullName;
        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(_fullName)) return _fullName;
                return string.Format("{0} {1}", Name.ToStr(), Surname.ToStr()).Trim();
            }
            set { _fullName = value; }
        }

        private string _address;
        public string Address
        {
            get
            {
                if (!string.IsNullOrEmpty(_address)) return _address;
                return string.Format("{0} {6} {1} {6} {2} {6} {3} {6} {4} {6} {5} {6}",
                    Street.ToStr(),
                    District.ToStr(),
                    Town.ToStr(),
                    City.ToStr(),
                    Country.ToStr(),
                    Description.ToStr(),
                    Environment.NewLine);
            }
            set { _address = value; }
        }

        private string _registrationAddress;
        public string RegistrationAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(_registrationAddress)) return _registrationAddress;
                return string.Format("{0} {1}, {2}, {3} {4}, {5}. {6}",
                    Street.ToStr(),
                    District.ToStr(),
                    Town.ToStr(),
                    ZipCode.ToStr(),
                    City.ToStr(),
                    Country.ToStr(),
                    Description.ToStr());
            }
            set { _registrationAddress = value; }
        }

        public bool isValidCustomer()
        {
            return !string.IsNullOrEmpty(Name)
                   && !string.IsNullOrEmpty(Surname)
                   && !string.IsNullOrEmpty(GsmNumber.ToStr()) && GeneralHelper.IsGsmNumberValid(GsmNumber.ToStr())
                   && !string.IsNullOrEmpty(Email.ToStr()) && GeneralHelper.IsValidEmail(Email.ToStr())
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
                   || string.IsNullOrEmpty(Country);
        }

        public EImece.Domain.Entities.Customer ToEntity()
        {
            return new EImece.Domain.Entities.Customer
            {
                Id = Id,
                Name = Name,
                Surname = Surname,
                Email = Email,
                GsmNumber = GsmNumber,
                IdentityNumber = IdentityNumber,
                City = City,
                Town = Town,
                District = District,
                Street = Street,
                ZipCode = ZipCode,
                Country = Country,
                Description = Description,
                Ip = Ip,
                UserId = UserId,
                CustomerType = CustomerType,
                BirthDate = BirthDate,
                Position = Position,
                Lang = Lang,
                IsActive = IsActive,
                CreatedDate = CreatedDate,
                UpdatedDate = UpdatedDate
            };
        }
    }
}
