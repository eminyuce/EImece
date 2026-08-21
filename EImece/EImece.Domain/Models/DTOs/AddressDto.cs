using EImece.Domain.Helpers;
using System;

namespace EImece.Domain.Models.DTOs
{
    public class AddressDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from Address
        public string Description { get; set; }
        public int AddressType { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string District { get; set; }

        public string AddressInfo
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

        public bool EqualsAddress(AddressDto other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(this.Street, other.Street, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.District, other.District, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.ZipCode, other.ZipCode, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.City, other.City, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.Country, other.Country, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        }

        public EImece.Domain.Entities.Address ToEntity()
        {
            return new EImece.Domain.Entities.Address
            {
                Id = Id,
                Name = Name,
                Description = Description,
                AddressType = AddressType,
                City = City,
                Country = Country,
                ZipCode = ZipCode,
                Street = Street,
                District = District,
                Position = Position,
                Lang = Lang,
                IsActive = IsActive,
                CreatedDate = CreatedDate,
                UpdatedDate = UpdatedDate
            };
        }
    }
}