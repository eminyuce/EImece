using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Read model representing storefront faceted search / category filters (brands, price range, ratings, specs).
    /// </summary>
    public class StorefrontFilterDto
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public List<StorefrontBrandDto> AvailableBrands { get; set; }
        public List<int> SelectedBrandIds { get; set; }
        public List<int> SelectedRatings { get; set; }

        public StorefrontFilterDto()
        {
            AvailableBrands = new List<StorefrontBrandDto>();
            SelectedBrandIds = new List<int>();
            SelectedRatings = new List<int>();
        }
    }
}
