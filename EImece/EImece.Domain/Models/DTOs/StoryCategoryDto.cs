using System;

namespace EImece.Domain.Models.DTOs
{
    public class StoryCategoryDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from BaseContent
        public string Description { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public int? MainImageId { get; set; }
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }
        public string UpdateUserId { get; set; }
        public string AddUserId { get; set; }

        // from StoryCategory
        public string PageTheme { get; set; }
        public string DetailPageAbsoluteUrl { get; set; }
        public string DetailPageRelativeUrl { get; set; }
    }
}
