namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected specification item for storefront product detail.
    /// </summary>
    public class StorefrontProductSpecificationDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
    }
}
