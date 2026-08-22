using System;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Minimal customer header for storefront account pages. Only fields shown in _CustomerDetails.cshtml.
    /// Query: SELECT Id, Name, Surname, Email, CreatedDate FROM Customers WHERE UserId=@id (plus aggregated order stats)
    /// </summary>
    public class CustomerSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalOrderCount { get; set; }
        public decimal TotalPaid { get; set; }
        public string UserId { get; set; }

        public string FullName => $"{Name} {Surname}".Trim();
    }
}
