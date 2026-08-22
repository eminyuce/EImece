namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Minimal shipping/billing address for order detail — only fields in AddressInfo.
    /// Projection: SELECT District, Street, ZipCode, Description, City, Country, AddressType FROM Addresses WHERE Id=@id (7 cols).
    /// Omits Id, Name, CreatedDate, UpdatedDate, IsActive, Position, Lang — never displayed in order detail.
    /// </summary>
    public class AddressSummaryDto
    {
        public string District { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int AddressType { get; set; }

        public string AddressInfo => $"{District} {Street} {ZipCode} {Description} {City} {Country}".Trim();
    }
}
