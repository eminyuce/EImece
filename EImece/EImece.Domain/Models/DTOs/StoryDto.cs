using System;

namespace EImece.Domain.Models.DTOs
{
    public class StoryDto
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

        // from Story
        public int StoryCategoryId { get; set; }
        public bool MainPage { get; set; }
        public string AuthorName { get; set; }
        public bool IsFeaturedStory { get; set; }
        public string ShortDescription { get; set; }
        public string DetailPageUrl { get; set; }
    }
}
