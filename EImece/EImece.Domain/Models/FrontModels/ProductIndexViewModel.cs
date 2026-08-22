using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductIndexViewModel : ItemListing
    {
        public PaginatedList<StorefrontProductCardDto> Products { get; set; }

        public SettingValueDto CompanyName { get; set; }

        public List<StorefrontTagDto> Tags { get; set; }

        public StorefrontMenuDto ProductMenu { get; set; }

        public StorefrontMenuDto MainPageMenu { get; set; }
    }
}