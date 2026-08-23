using EImece.Domain.Helpers;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Purpose-specific read model for storefront product detail pages.
    /// Includes full description, active files, active tags, specifications, and approved comments.
    /// </summary>
    public class StorefrontProductDetailDto : StorefrontProductCardDto
    {
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string ProductColorOptions { get; set; }
        public string ProductSizeOptions { get; set; }
        public string MetaKeywords { get; set; }
        public int? ProductCategoryTemplateId { get; set; }
        public StorefrontCategoryDto ProductCategory { get; set; }
        public StorefrontBrandDto Brand { get; set; }

        public List<StorefrontProductFileDto> ProductFiles { get; set; }
        public List<StorefrontTagDto> ProductTags { get; set; }
        public List<StorefrontProductSpecificationDto> ProductSpecifications { get; set; }
        public List<StorefrontProductCommentDto> ProductComments { get; set; }

        public StorefrontProductDetailDto()
        {
            ProductFiles = new List<StorefrontProductFileDto>();
            ProductTags = new List<StorefrontTagDto>();
            ProductSpecifications = new List<StorefrontProductSpecificationDto>();
            ProductComments = new List<StorefrontProductCommentDto>();
        }
    }
}
