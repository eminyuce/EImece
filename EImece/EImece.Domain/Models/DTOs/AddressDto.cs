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
    }
}