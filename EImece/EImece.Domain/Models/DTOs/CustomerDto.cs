using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs
{
    public class CustomerDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from Customer
        public string Surname { get; set; }
        public string GsmNumber { get; set; }
        public string Email { get; set; }
        public string IdentityNumber { get; set; }
        public string Ip { get; set; }
        public bool IsSameAsShippingAddress { get; set; }
        public string UserId { get; set; }
        public bool IsPermissionGranted { get; set; }
        public int Gender { get; set; }
        public string Street { get; set; }
        public string Town { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string Description { get; set; }
        public string Company { get; set; }
        public int CustomerType { get; set; }
        public String Captcha { get; set; }
        public DateTime OrderLatestDate { get; set; }
        public String FullName { get; set; }
        public string Address { get; set; }
        public string RegistrationAddress { get; set; }
    }
}
