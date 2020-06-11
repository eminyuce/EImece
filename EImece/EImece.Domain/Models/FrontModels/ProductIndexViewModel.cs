using EImece.Domain.Entities;
using GenericRepository;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductIndexViewModel
    {
        public PaginatedList<Product> Products { get; set; }

        public Setting CompanyName { get; set; }

        public List<Tag> Tags { get; set; }

        public Menu ProductMenu { get; set; }

        public Menu MainPageMenu { get; set; }

    }
}
