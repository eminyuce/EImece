using System;

namespace EImece.Domain.Models.DTOs
{
    public class ProductCommentDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from ProductComment
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string Review { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public int Rating { get; set; }
        public string SeoUrl { get; set; }
    }
}
