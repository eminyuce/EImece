using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.DTOs
{
    public class ProductCategoryDto
    {
        // From BaseEntity
        public int Id { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Name))]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsActive))]
        public bool IsActive { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Position))]
        public int Position { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.LanguageLabel))]
        public int Lang { get; set; }

        // From BaseContent
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Description))]
        public string Description { get; set; }

        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public int? MainImageId { get; set; }

        // ProductCategory-specific fields
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ParentIdLabel))]
        public int ParentId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MainPage))]
        public bool MainPage { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ShortDescription))]
        public string ShortDescription { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.TemplateIdLabel))]
        public int? TemplateId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.DiscountPercentageLabel))]
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
