using System;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected approved product comment for storefront display.
    /// </summary>
    public class StorefrontProductCommentDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
