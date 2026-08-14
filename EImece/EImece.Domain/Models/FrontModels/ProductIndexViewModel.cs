using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductIndexViewModel : ItemListing
    {
        public PaginatedList<StorefrontProductCardDto> Products { get; set; }

        public Setting CompanyName { get; set; }

        public List<Tag> Tags { get; set; }

        public Menu ProductMenu { get; set; }

        public Menu MainPageMenu { get; set; }
    }
}