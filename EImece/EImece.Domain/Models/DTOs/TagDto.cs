using System;

namespace EImece.Domain.Models.DTOs
{
    public class TagDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from Tag
        public int TagCategoryId { get; set; }
        public string DetailPageRelativeUrlForProducts { get; set; }
        public string DetailPageRelativeUrlForStories { get; set; }
    }
}
