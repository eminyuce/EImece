using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs
{
    public class ProductCategoryDto
    {
        // From BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // From BaseContent
        public string Description { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public int? MainImageId { get; set; }

        // ProductCategory-specific fields
        public int ParentId { get; set; }
        public bool MainPage { get; set; }
        public string ShortDescription { get; set; }
        public int? TemplateId { get; set; }
        public double? DiscountPercentage { get; set; }   // fixed typo: Percantage → Percentage

        // Optional: flattened / useful computed fields for frontend
        public string MainImageUrl { get; set; }           // e.g. full or cropped URL
        public string MainImageThumbnailUrl { get; set; }  // smaller version
        public string SeoUrl { get; set; }                 // friendly URL slug
        public string DetailPageUrl { get; set; }          // full frontend URL

        // If you need hierarchy info (common for category trees)
        public List<ProductCategoryDto> Children { get; set; } = new List<ProductCategoryDto>();
        // ParentName or ParentSeoUrl can be added if needed, but usually avoided in flat DTOs
    }
}